using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.Services;

/// <summary>
/// Creates plug-in packages from descriptors discovered at runtime.
/// </summary>
public sealed class PluginExportService : IPluginExportService
{
    private readonly PluginLoaderOptions _options;
    private readonly IServiceCatalog _serviceCatalog;
    private readonly ILogger<PluginExportService> _logger;

    public PluginExportService(
        PluginLoaderOptions options,
        IServiceCatalog serviceCatalog,
        ILogger<PluginExportService> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _serviceCatalog = serviceCatalog ?? throw new ArgumentNullException(nameof(serviceCatalog));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IReadOnlyCollection<PluginDescriptorExportInfo> GetExportableDescriptors()
    {
        var (index, _) = BuildModuleIndex();
        return index.Values
            .Select(context => new PluginDescriptorExportInfo(
                context.Descriptor.Id,
                context.Descriptor.DisplayName,
                context.Descriptor.Category))
            .OrderBy(info => info.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<PluginExportResult> ExportAsync(PluginExportRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            return await Task.Run(() => ExportInternal(request, cancellationToken), cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Plug-in export cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export plug-in package for descriptors {Descriptors}.", string.Join(", ", request.DescriptorIds ?? Array.Empty<string>()));
            return new PluginExportResult(false, "Export failed. Check logs for details.", null, request.DescriptorIds ?? Array.Empty<string>());
        }
    }

    private PluginExportResult ExportInternal(PluginExportRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PluginId))
        {
            return new PluginExportResult(false, "A plug-in id is required.", null, request.DescriptorIds ?? Array.Empty<string>());
        }

        if (string.IsNullOrWhiteSpace(request.PluginName))
        {
            return new PluginExportResult(false, "A plug-in name is required.", null, request.DescriptorIds ?? Array.Empty<string>());
        }

        if (string.IsNullOrWhiteSpace(request.Version) || !Version.TryParse(request.Version, out _))
        {
            return new PluginExportResult(false, "Provide a semantic version (for example, 1.0.0).", null, request.DescriptorIds ?? Array.Empty<string>());
        }

        var descriptorIds = request.DescriptorIds ?? Array.Empty<string>();
        if (descriptorIds.Count == 0)
        {
            return new PluginExportResult(false, "Select at least one descriptor to export.", null, descriptorIds);
        }

        var (moduleIndex, moduleDiagnostics) = BuildModuleIndex();
        var selectedContexts = new List<ModuleExportContext>();
        foreach (var descriptorId in descriptorIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!moduleIndex.TryGetValue(descriptorId, out var context))
            {
                return new PluginExportResult(false, $"Descriptor '{descriptorId}' is not available for export.", null, descriptorIds)
                {
                    Diagnostics = moduleDiagnostics,
                };
            }

            selectedContexts.Add(context);
        }

        if (selectedContexts.Count == 0)
        {
            return new PluginExportResult(false, "No descriptors matched the selection.", null, descriptorIds)
            {
                Diagnostics = moduleDiagnostics,
            };
        }

        var filesToCopy = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var serviceAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var probingPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string? entryAssembly = null;

        foreach (var context in selectedContexts)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var moduleRelativePath = NormalizeRelativePath(context.PluginRoot, context.AssemblyPath);
            AddFile(moduleRelativePath, context.AssemblyPath, filesToCopy);
            serviceAssemblies.Add(moduleRelativePath);
            entryAssembly ??= moduleRelativePath;

            if (context.Manifest is not null)
            {
                IncludeFromManifest(context, filesToCopy, serviceAssemblies, probingPaths, cancellationToken);
            }
            else
            {
                IncludeDirectoryContents(Path.GetDirectoryName(context.AssemblyPath)!, context.PluginRoot, filesToCopy, cancellationToken);
            }
        }

        if (filesToCopy.Count == 0)
        {
            return new PluginExportResult(false, "No assemblies were discovered for the selected descriptors.", null, descriptorIds)
            {
                Diagnostics = moduleDiagnostics,
            };
        }

        entryAssembly ??= serviceAssemblies.First();
        var manifest = BuildManifest(request, entryAssembly, serviceAssemblies, probingPaths);
        if (!PluginPackageUtilities.TryValidateBasicManifest(manifest, "export", out var manifestErrors))
        {
            return new PluginExportResult(false, string.Join(Environment.NewLine, manifestErrors), null, descriptorIds)
            {
                Diagnostics = moduleDiagnostics,
            };
        }

        var destination = ResolveDestinationPath(request);
        cancellationToken.ThrowIfCancellationRequested();

        var tempRoot = CreateTemporaryDirectory();
        try
        {
            foreach (var kvp in filesToCopy)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var targetPath = Path.Combine(tempRoot, kvp.Key);
                var targetDirectory = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                File.Copy(kvp.Value, targetPath, overwrite: true);
            }

            var manifestPath = Path.Combine(tempRoot, PluginPackageUtilities.ManifestFileName);
            using (var stream = File.Create(manifestPath))
            {
                JsonSerializer.Serialize(stream, manifest, PluginPackageUtilities.SerializerOptions);
            }

            var destinationDirectory = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            if (File.Exists(destination))
            {
                File.Delete(destination);
            }

            ZipFile.CreateFromDirectory(tempRoot, destination, CompressionLevel.Optimal, includeBaseDirectory: false);
        }
        finally
        {
            TryDeleteDirectory(tempRoot);
        }

        var successMessage = selectedContexts.Count == 1
            ? $"Exported descriptor '{selectedContexts[0].Descriptor.DisplayName}'."
            : $"Exported {selectedContexts.Count} descriptors.";

        if (moduleDiagnostics.Count > 0)
        {
            successMessage += " Some modules were skipped due to load errors.";
        }

        _logger.LogInformation("{Message} Package: {PackagePath}.", successMessage, destination);
        return new PluginExportResult(true, successMessage, destination, descriptorIds)
        {
            Diagnostics = moduleDiagnostics,
        };
    }

    private void IncludeFromManifest(
        ModuleExportContext context,
        IDictionary<string, string> filesToCopy,
        ISet<string> serviceAssemblies,
        ISet<string> probingPaths,
        CancellationToken cancellationToken)
    {
        var manifest = context.Manifest!;

        var entryAssemblyPath = PluginPackageUtilities.ResolveRelativePath(context.PluginRoot, manifest.EntryAssembly);
        if (entryAssemblyPath is not null && File.Exists(entryAssemblyPath))
        {
            var relative = NormalizeRelativePath(context.PluginRoot, entryAssemblyPath);
            AddFile(relative, entryAssemblyPath, filesToCopy);
            serviceAssemblies.Add(relative);
        }

        foreach (var assemblyReference in manifest.ServiceAssemblies)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var resolved = PluginPackageUtilities.ResolveRelativePath(context.PluginRoot, assemblyReference);
            if (resolved is null || !File.Exists(resolved))
            {
                continue;
            }

            var relative = NormalizeRelativePath(context.PluginRoot, resolved);
            AddFile(relative, resolved, filesToCopy);
            serviceAssemblies.Add(relative);
        }

        foreach (var probingPath in manifest.ProbingPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var resolved = PluginPackageUtilities.ResolveRelativePath(context.PluginRoot, probingPath);
            if (resolved is null || !Directory.Exists(resolved))
            {
                continue;
            }

            var relativeDirectory = NormalizeRelativePath(context.PluginRoot, resolved);
            probingPaths.Add(relativeDirectory);
            IncludeDirectoryContents(resolved, context.PluginRoot, filesToCopy, cancellationToken);
        }
    }

    private static void IncludeDirectoryContents(
        string directory,
        string pluginRoot,
        IDictionary<string, string> filesToCopy,
        CancellationToken cancellationToken)
    {
        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.Equals(Path.GetFileName(file), PluginPackageUtilities.ManifestFileName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!PluginPackageUtilities.IsPathWithinRoot(pluginRoot, file))
            {
                continue;
            }

            var relative = NormalizeRelativePath(pluginRoot, file);
            AddFile(relative, file, filesToCopy);
        }
    }

    private static void AddFile(string relativePath, string sourcePath, IDictionary<string, string> filesToCopy)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            relativePath = Path.GetFileName(sourcePath);
        }

        if (filesToCopy.TryGetValue(relativePath, out var existing))
        {
            if (!PluginPackageUtilities.PathsEqual(existing, sourcePath))
            {
                throw new InvalidOperationException(FormattableString.Invariant($"Conflicting files map to '{relativePath}'."));
            }

            return;
        }

        filesToCopy[relativePath] = sourcePath;
    }

    private PluginManifest BuildManifest(
        PluginExportRequest request,
        string entryAssembly,
        IEnumerable<string> serviceAssemblies,
        IEnumerable<string> probingPaths)
    {
        var normalizedEntry = NormalizeForManifest(entryAssembly);
        var services = serviceAssemblies
            .Where(path => !string.Equals(path, entryAssembly, StringComparison.OrdinalIgnoreCase))
            .Select(NormalizeForManifest)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var normalizedProbing = probingPaths
            .Select(NormalizeForManifest)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new PluginManifest
        {
            Id = request.PluginId.Trim(),
            Name = request.PluginName.Trim(),
            Version = request.Version.Trim(),
            EntryAssembly = normalizedEntry,
            ServiceAssemblies = services,
            ProbingPaths = normalizedProbing,
        };
    }

    private string ResolveDestinationPath(PluginExportRequest request)
    {
        var destination = request.DestinationPath;
        if (string.IsNullOrWhiteSpace(destination))
        {
            var safeId = PluginPackageUtilities.SanitizeSegment(request.PluginId.Trim());
            var safeVersion = PluginPackageUtilities.SanitizeSegment(request.Version.Trim());
            var fileName = FormattableString.Invariant($"{safeId}-{safeVersion}.peakiot");
            return _options.GetPackagePath(fileName);
        }

        destination = destination.Trim();
        var extension = Path.GetExtension(destination);
        if (string.IsNullOrWhiteSpace(extension) || !extension.Equals(".peakiot", StringComparison.OrdinalIgnoreCase))
        {
            destination = Path.ChangeExtension(destination, ".peakiot");
        }

        return destination;
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "PluginExport", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup failures so export result is not hidden.
        }
    }

    private (Dictionary<string, ModuleExportContext> Index, IReadOnlyCollection<string> Diagnostics) BuildModuleIndex()
    {
        var assemblies = PluginLoader.CombineWithDefaultAssemblies(AppDomain.CurrentDomain.GetAssemblies());
        var diagnostics = new List<string>();
        var modules = ServiceModuleDiscovery.InstantiateModules(
            assemblies,
            (assembly, errors) =>
            {
                var errorArray = errors?.Where(static error => error is not null).Cast<Exception>().ToArray()
                    ?? Array.Empty<Exception>();
                if (errorArray.Length == 0)
                {
                    return;
                }

                var diagnostic = FormatTypeLoadDiagnostic(assembly, errorArray);
                diagnostics.Add(diagnostic);

                var assemblyName = assembly?.GetName().Name ?? assembly?.FullName ?? "(unknown)";
                var summary = string.Join("; ", errorArray.Select(error => error.Message));
                _logger.LogWarning(
                    errorArray[0],
                    "Failed to load module types from assembly {AssemblyName}. Errors: {ErrorSummary}.",
                    assemblyName,
                    summary);

                for (var i = 1; i < errorArray.Length; i++)
                {
                    _logger.LogDebug(
                        errorArray[i],
                        "Additional loader error for assembly {AssemblyName}.",
                        assemblyName);
                }
            });

        var result = new Dictionary<string, ModuleExportContext>(StringComparer.Ordinal);
        var availableIds = new HashSet<string>(_serviceCatalog.Descriptors.Select(d => d.Id), StringComparer.Ordinal);

        foreach (var module in modules)
        {
            var moduleAssembly = module.GetType().Assembly;
            var location = moduleAssembly.Location;
            if (string.IsNullOrWhiteSpace(location))
            {
                continue;
            }

            if (!IsWithinManagedRoots(location))
            {
                continue;
            }

            var pluginRoot = FindPluginRoot(location);
            if (pluginRoot is null)
            {
                continue;
            }

            PluginManifest? manifest = null;
            var manifestPath = _options.GetManifestPath(pluginRoot);
            if (File.Exists(manifestPath))
            {
                try
                {
                    using var manifestStream = File.OpenRead(manifestPath);
                    manifest = JsonSerializer.Deserialize<PluginManifest>(manifestStream, PluginPackageUtilities.SerializerOptions);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read plug-in manifest at {ManifestPath}.", manifestPath);
                }
            }

            foreach (var descriptor in module.DescribeServices() ?? Array.Empty<IServiceDescriptor>())
            {
                if (descriptor is null)
                {
                    continue;
                }

                if (!availableIds.Contains(descriptor.Id))
                {
                    continue;
                }

                result[descriptor.Id] = new ModuleExportContext(module, descriptor, location, pluginRoot, manifest);
            }
        }

        return (result, diagnostics.ToArray());
    }

    private bool IsWithinManagedRoots(string path)
    {
        return PluginPackageUtilities.IsPathWithinRoot(_options.ExtractionDirectory, path)
            || PluginPackageUtilities.IsPathWithinRoot(_options.RootDirectory, path);
    }

    private string? FindPluginRoot(string assemblyPath)
    {
        var directory = Path.GetDirectoryName(assemblyPath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return null;
        }

        var current = directory;
        while (!string.IsNullOrEmpty(current))
        {
            var manifestPath = _options.GetManifestPath(current);
            if (File.Exists(manifestPath))
            {
                return current;
            }

            if (!IsWithinManagedRoots(current))
            {
                break;
            }

            var parent = Directory.GetParent(current)?.FullName;
            if (string.IsNullOrEmpty(parent) || PluginPackageUtilities.PathsEqual(parent, current))
            {
                break;
            }

            current = parent;
        }

        return null;
    }

    private static string NormalizeRelativePath(string root, string path)
    {
        var relative = Path.GetRelativePath(root, path);
        if (string.Equals(relative, ".", StringComparison.Ordinal))
        {
            return Path.GetFileName(path);
        }

        return relative;
    }

    private static string NormalizeForManifest(string relativePath)
    {
        return relativePath.Replace(Path.DirectorySeparatorChar, '/');
    }

    private static string FormatTypeLoadDiagnostic(Assembly? assembly, IReadOnlyCollection<Exception> errors)
    {
        var assemblyName = assembly?.GetName().Name ?? assembly?.FullName ?? "(unknown)";
        var summary = string.Join("; ", errors.Select(error => error.Message));
        return FormattableString.Invariant($"Assembly '{assemblyName}' skipped: {summary}");
    }

    private sealed record ModuleExportContext(
        IServiceModule Module,
        IServiceDescriptor Descriptor,
        string AssemblyPath,
        string PluginRoot,
        PluginManifest? Manifest);
}
