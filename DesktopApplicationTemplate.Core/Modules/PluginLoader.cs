using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.Core.Modules;

/// <summary>
/// Loads plug-in packages and exposes their assemblies for service module discovery.
/// </summary>
public sealed class PluginLoader
{
    private static readonly List<PluginLoadContext> ActiveContexts = new();
    private static readonly HashSet<string> SupportedArchiveExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".peakiot",
        ".zip",
    };

    private readonly PluginLoaderOptions options;
    private readonly ILogger<PluginLoader> logger;
    private readonly HashSet<string> loadedPlugins = new(StringComparer.OrdinalIgnoreCase);

    public PluginLoader(PluginLoaderOptions options, ILogger<PluginLoader> logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a loader using configuration derived options.
    /// </summary>
    /// <param name="configuration">The configuration source.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>The configured <see cref="PluginLoader"/> instance.</returns>
    public static PluginLoader Create(IConfiguration configuration, ILogger<PluginLoader> logger)
    {
        var options = PluginLoaderOptions.FromConfiguration(configuration);
        return new PluginLoader(options, logger);
    }

    /// <summary>
    /// Gets the loader options.
    /// </summary>
    public PluginLoaderOptions Options => options;

    /// <summary>
    /// Loads plug-in assemblies from the configured directories.
    /// </summary>
    /// <returns>The assemblies that should be scanned for service modules.</returns>
    public IReadOnlyCollection<Assembly> LoadPluginAssemblies()
    {
        var assemblies = new List<Assembly>();

        if (!Directory.Exists(options.RootDirectory))
        {
            logger.LogDebug("Plug-in directory {PluginRoot} does not exist. No plug-ins will be loaded.", options.RootDirectory);
            return assemblies;
        }

        foreach (var entry in Directory.EnumerateFileSystemEntries(options.RootDirectory))
        {
            try
            {
                if (Directory.Exists(entry))
                {
                    LoadFromDirectory(entry, assemblies);
                }
                else if (SupportedArchiveExtensions.Contains(Path.GetExtension(entry)))
                {
                    LoadFromArchive(entry, assemblies);
                }
                else
                {
                    logger.LogDebug("Skipping unsupported plug-in payload {PluginPath}.", entry);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load plug-in payload {PluginPath}.", entry);
            }
        }

        return new ReadOnlyCollection<Assembly>(assemblies);
    }

    /// <summary>
    /// Combines plug-in assemblies with the currently loaded application assemblies to support module discovery.
    /// </summary>
    /// <param name="pluginAssemblies">Assemblies discovered by <see cref="LoadPluginAssemblies"/>.</param>
    /// <returns>A distinct set of assemblies for module scanning.</returns>
    public static IReadOnlyCollection<Assembly> CombineWithDefaultAssemblies(IEnumerable<Assembly> pluginAssemblies)
    {
        var defaultAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var combined = defaultAssemblies
            .Concat(pluginAssemblies ?? Array.Empty<Assembly>())
            .Where(a => a is not null)
            .GroupBy(a => a.FullName, StringComparer.Ordinal)
            .Select(g => g.First())
            .ToArray();

        return combined;
    }

    private void LoadFromArchive(string archivePath, ICollection<Assembly> assemblies)
    {
        using var archive = ZipFile.OpenRead(archivePath);
        var manifestEntry = archive.GetEntry(PluginPackageUtilities.ManifestFileName);
        if (manifestEntry is null)
        {
            logger.LogWarning("Plug-in archive {ArchivePath} does not contain {ManifestFileName}.", archivePath, PluginPackageUtilities.ManifestFileName);
            return;
        }

        PluginManifest? manifest;
        using (var manifestStream = manifestEntry.Open())
        {
            manifest = JsonSerializer.Deserialize<PluginManifest>(manifestStream, PluginPackageUtilities.SerializerOptions);
        }

        if (!PluginPackageUtilities.TryValidateBasicManifest(manifest, archivePath, out var basicErrors))
        {
            foreach (var error in basicErrors)
            {
                logger.LogWarning("{ManifestError}", error);
            }

            return;
        }

        var extractionPath = options.GetExtractionPath(manifest!);
        if (Directory.Exists(extractionPath))
        {
            Directory.Delete(extractionPath, recursive: true);
        }

        Directory.CreateDirectory(extractionPath);
        archive.ExtractToDirectory(extractionPath, overwriteFiles: true);

        LoadFromDirectory(extractionPath, assemblies, archivePath);
    }

    private void LoadFromDirectory(string directoryPath, ICollection<Assembly> assemblies, string? origin = null)
    {
        var manifestPath = options.GetManifestPath(directoryPath);
        if (!File.Exists(manifestPath))
        {
            logger.LogWarning("Plug-in folder {PluginDirectory} does not contain {ManifestFileName}.", directoryPath, PluginPackageUtilities.ManifestFileName);
            return;
        }

        PluginManifest? manifest;
        try
        {
            using var stream = File.OpenRead(manifestPath);
            manifest = JsonSerializer.Deserialize<PluginManifest>(stream, PluginPackageUtilities.SerializerOptions);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Plug-in manifest {ManifestPath} is invalid JSON.", manifestPath);
            return;
        }

        if (!PluginPackageUtilities.TryValidateManifest(manifest, directoryPath, origin ?? manifestPath, out var errors))
        {
            foreach (var error in errors)
            {
                logger.LogWarning("{ManifestError}", error);
            }

            return;
        }

        var manifestKey = PluginPackageUtilities.CreateManifestKey(manifest!);
        if (!loadedPlugins.Add(manifestKey))
        {
            logger.LogInformation("Plug-in {PluginId} version {PluginVersion} was already loaded. Skipping payload at {PluginDirectory}.", manifest!.Id, manifest.Version, directoryPath);
            return;
        }

        var entryAssemblyPath = PluginPackageUtilities.ResolveRelativePath(directoryPath, manifest!.EntryAssembly);
        if (entryAssemblyPath is null)
        {
            logger.LogWarning("Entry assembly {EntryAssembly} for plug-in {PluginId} resolves outside of the plug-in directory.", manifest.EntryAssembly, manifest.Id);
            return;
        }

        var loadContext = new PluginLoadContext(entryAssemblyPath, directoryPath, manifest.ProbingPaths);
        ActiveContexts.Add(loadContext);

        var assemblyReferences = manifest.ServiceAssemblies.Count > 0
            ? manifest.ServiceAssemblies
            : new List<string> { manifest.EntryAssembly };

        foreach (var assemblyReference in assemblyReferences)
        {
            var assemblyPath = PluginPackageUtilities.ResolveRelativePath(directoryPath, assemblyReference);
            if (assemblyPath is null || !File.Exists(assemblyPath))
            {
                logger.LogWarning(
                    "Plug-in {PluginId} references assembly {AssemblyReference} that could not be resolved.",
                    manifest.Id,
                    assemblyReference);
                continue;
            }

            try
            {
                var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);
                assemblies.Add(assembly);
                logger.LogInformation(
                    "Loaded plug-in assembly {AssemblyName} for {PluginId} version {PluginVersion}.",
                    assembly.GetName().Name,
                    manifest.Id,
                    manifest.Version);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to load plug-in assembly {AssemblyPath} for {PluginId}.",
                    assemblyPath,
                    manifest.Id);
            }
        }
    }

    private sealed class PluginLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver resolver;
        private readonly IReadOnlyList<string> probingPaths;

        public PluginLoadContext(string entryAssemblyPath, string pluginDirectory, IEnumerable<string> probingPaths)
            : base(isCollectible: false)
        {
            if (string.IsNullOrWhiteSpace(entryAssemblyPath))
            {
                throw new ArgumentException("Entry assembly path must be provided", nameof(entryAssemblyPath));
            }

            resolver = new AssemblyDependencyResolver(entryAssemblyPath);
            var rootPath = PluginPackageUtilities.EnsureTrailingSeparator(Path.GetFullPath(pluginDirectory));
            this.probingPaths = probingPaths
                .Select(path => PluginPackageUtilities.ResolveRelativePath(rootPath, path))
                .Where(path => path is not null)
                .Cast<string>()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var resolvedPath = resolver.ResolveAssemblyToPath(assemblyName);
            if (!string.IsNullOrEmpty(resolvedPath))
            {
                return LoadFromAssemblyPath(resolvedPath);
            }

            foreach (var probingPath in probingPaths)
            {
                var candidate = Path.Combine(probingPath, assemblyName.Name + ".dll");
                if (File.Exists(candidate))
                {
                    return LoadFromAssemblyPath(candidate);
                }
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var resolvedPath = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (!string.IsNullOrEmpty(resolvedPath))
            {
                return LoadUnmanagedDllFromPath(resolvedPath);
            }

            foreach (var probingPath in probingPaths)
            {
                var candidate = Path.Combine(probingPath, unmanagedDllName + ".dll");
                if (File.Exists(candidate))
                {
                    return LoadUnmanagedDllFromPath(candidate);
                }
            }

            return IntPtr.Zero;
        }
    }
}
