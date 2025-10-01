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
    private const string ManifestFileName = "plugin.manifest.json";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private static readonly List<PluginLoadContext> ActiveContexts = new();
    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;
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
        var manifestEntry = archive.GetEntry(ManifestFileName);
        if (manifestEntry is null)
        {
            logger.LogWarning("Plug-in archive {ArchivePath} does not contain {ManifestFileName}.", archivePath, ManifestFileName);
            return;
        }

        PluginManifest? manifest;
        using (var manifestStream = manifestEntry.Open())
        {
            manifest = JsonSerializer.Deserialize<PluginManifest>(manifestStream, SerializerOptions);
        }

        if (!TryValidateBasicManifest(manifest, archivePath, out var basicErrors))
        {
            foreach (var error in basicErrors)
            {
                logger.LogWarning("{ManifestError}", error);
            }

            return;
        }

        var extractionPath = GetExtractionPath(manifest!);
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
        var manifestPath = Path.Combine(directoryPath, ManifestFileName);
        if (!File.Exists(manifestPath))
        {
            logger.LogWarning("Plug-in folder {PluginDirectory} does not contain {ManifestFileName}.", directoryPath, ManifestFileName);
            return;
        }

        PluginManifest? manifest;
        try
        {
            using var stream = File.OpenRead(manifestPath);
            manifest = JsonSerializer.Deserialize<PluginManifest>(stream, SerializerOptions);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Plug-in manifest {ManifestPath} is invalid JSON.", manifestPath);
            return;
        }

        if (!TryValidateManifest(manifest, directoryPath, origin ?? manifestPath, out var errors))
        {
            foreach (var error in errors)
            {
                logger.LogWarning("{ManifestError}", error);
            }

            return;
        }

        var manifestKey = CreateManifestKey(manifest!);
        if (!loadedPlugins.Add(manifestKey))
        {
            logger.LogInformation("Plug-in {PluginId} version {PluginVersion} was already loaded. Skipping payload at {PluginDirectory}.", manifest!.Id, manifest.Version, directoryPath);
            return;
        }

        var entryAssemblyPath = ResolvePath(directoryPath, manifest!.EntryAssembly);
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
            var assemblyPath = ResolvePath(directoryPath, assemblyReference);
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

    private static bool TryValidateBasicManifest(PluginManifest? manifest, string origin, out List<string> errors)
    {
        errors = new List<string>();
        if (manifest is null)
        {
            errors.Add(FormattableString.Invariant($"Plug-in manifest in '{origin}' could not be parsed."));
            return false;
        }

        if (string.IsNullOrWhiteSpace(manifest.Id))
        {
            errors.Add(FormattableString.Invariant($"Plug-in manifest in '{origin}' is missing 'id'."));
        }

        if (string.IsNullOrWhiteSpace(manifest.Version))
        {
            errors.Add(FormattableString.Invariant($"Plug-in manifest in '{origin}' is missing 'version'."));
        }
        else if (!Version.TryParse(manifest.Version, out _))
        {
            errors.Add(FormattableString.Invariant($"Plug-in manifest in '{origin}' specifies an invalid version '{manifest.Version}'."));
        }

        if (string.IsNullOrWhiteSpace(manifest.EntryAssembly))
        {
            errors.Add(FormattableString.Invariant($"Plug-in manifest in '{origin}' is missing 'entryAssembly'."));
        }

        return errors.Count == 0;
    }

    private static bool TryValidateManifest(PluginManifest? manifest, string pluginDirectory, string origin, out List<string> errors)
    {
        if (!TryValidateBasicManifest(manifest, origin, out errors))
        {
            return false;
        }

        if (manifest is null)
        {
            return false;
        }

        var entryAssembly = ResolvePath(pluginDirectory, manifest.EntryAssembly);
        if (entryAssembly is null)
        {
            errors.Add(FormattableString.Invariant($"Entry assembly '{manifest.EntryAssembly}' resolves outside '{pluginDirectory}'."));
        }
        else if (!File.Exists(entryAssembly))
        {
            errors.Add(FormattableString.Invariant($"Entry assembly '{manifest.EntryAssembly}' was not found at '{entryAssembly}'."));
        }

        foreach (var assemblyReference in manifest.ServiceAssemblies)
        {
            var resolved = ResolvePath(pluginDirectory, assemblyReference);
            if (resolved is null)
            {
                errors.Add(FormattableString.Invariant($"Service assembly '{assemblyReference}' resolves outside '{pluginDirectory}'."));
                continue;
            }

            if (!File.Exists(resolved))
            {
                errors.Add(FormattableString.Invariant($"Service assembly '{assemblyReference}' was not found at '{resolved}'."));
            }
        }

        foreach (var probingPath in manifest.ProbingPaths)
        {
            if (ResolvePath(pluginDirectory, probingPath) is null)
            {
                errors.Add(FormattableString.Invariant($"Probing path '{probingPath}' resolves outside '{pluginDirectory}'."));
            }
        }

        return errors.Count == 0;
    }

    private static string CreateManifestKey(PluginManifest manifest)
    {
        return FormattableString.Invariant($"{manifest.Id.Trim()}|{manifest.Version.Trim()}");
    }

    private string GetExtractionPath(PluginManifest manifest)
    {
        var safeId = SanitizeSegment(manifest.Id);
        var safeVersion = SanitizeSegment(manifest.Version);
        return Path.Combine(options.ExtractionDirectory, safeId, safeVersion);
    }

    private static string? ResolvePath(string root, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        var combined = Path.GetFullPath(Path.Combine(root, relativePath));
        var rootPath = EnsureTrailingSeparator(Path.GetFullPath(root));
        if (!combined.StartsWith(rootPath, PathComparison))
        {
            return null;
        }

        return combined;
    }

    private static string EnsureTrailingSeparator(string path)
    {
        if (!path.EndsWith(Path.DirectorySeparatorChar) && !path.EndsWith(Path.AltDirectorySeparatorChar))
        {
            path += Path.DirectorySeparatorChar;
        }

        return path;
    }

    private static string SanitizeSegment(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var buffer = value.ToCharArray();
        for (var i = 0; i < buffer.Length; i++)
        {
            if (invalid.Contains(buffer[i]))
            {
                buffer[i] = '_';
            }
        }

        return new string(buffer);
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
            var rootPath = EnsureTrailingSeparator(Path.GetFullPath(pluginDirectory));
            this.probingPaths = probingPaths
                .Select(path => ResolvePath(rootPath, path))
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
