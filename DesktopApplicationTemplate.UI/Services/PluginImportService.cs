using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.Services;

/// <summary>
/// Handles importing plug-in archives and refreshing the descriptor catalog.
/// </summary>
public sealed class PluginImportService : IPluginImportService
{
    private static readonly string[] SupportedExtensionList =
    {
        ".peakiot",
    };

    private static readonly HashSet<string> SupportedExtensions = new(SupportedExtensionList, StringComparer.OrdinalIgnoreCase);

    private readonly PluginLoaderOptions _options;
    private readonly IServiceCatalog _catalog;
    private readonly ILogger<PluginImportService> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public PluginImportService(
        PluginLoaderOptions options,
        IServiceCatalog catalog,
        ILogger<PluginImportService> logger,
        ILoggerFactory loggerFactory)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public Task<PluginImportResult> ImportAsync(string sourcePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return Task.FromResult(new PluginImportResult(false, "No plug-in package was selected.", null, Array.Empty<IServiceDescriptor>()));
        }

        var extension = Path.GetExtension(sourcePath);
        if (!SupportedExtensions.Contains(extension))
        {
            var message = $"Unsupported plug-in extension '{extension}'. Supported extensions: {string.Join(", ", SupportedExtensionList)}.";
            _logger.LogWarning(message);
            return Task.FromResult(new PluginImportResult(false, message, null, Array.Empty<IServiceDescriptor>()));
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            Directory.CreateDirectory(_options.RootDirectory);
            var fileName = Path.GetFileName(sourcePath);
            var destinationPath = Path.Combine(_options.RootDirectory, fileName);

            if (!PathsEqual(sourcePath, destinationPath))
            {
                _logger.LogInformation("Copying plug-in package {Source} to {Destination}.", sourcePath, destinationPath);
                File.Copy(sourcePath, destinationPath, overwrite: true);
            }

            var loaderLogger = _loggerFactory.CreateLogger<PluginLoader>();
            var loader = new PluginLoader(_options, loaderLogger);
            IReadOnlyCollection<Assembly> pluginAssemblies = loader.LoadPluginAssemblies();

            var modules = ServiceModuleDiscovery.InstantiateModules(pluginAssemblies);
            var descriptors = ServiceModuleDiscovery.DescribeServices(modules);

            if (descriptors.Count == 0)
            {
                var message = "The package did not expose any service descriptors.";
                _logger.LogWarning(message);
                return Task.FromResult(new PluginImportResult(false, message, destinationPath, Array.Empty<IServiceDescriptor>()));
            }

            var merged = new Dictionary<string, IServiceDescriptor>(StringComparer.Ordinal);
            foreach (var descriptor in _catalog.Descriptors)
            {
                merged[descriptor.Id] = descriptor;
            }

            foreach (var descriptor in descriptors)
            {
                merged[descriptor.Id] = descriptor;
            }

            _catalog.UpdateDescriptors(merged.Values);

            var successMessage = descriptors.Count == 1
                ? "Imported 1 service descriptor."
                : $"Imported {descriptors.Count} service descriptors.";
            _logger.LogInformation("{Message} Package: {PackagePath}.", successMessage, destinationPath);

            return Task.FromResult(new PluginImportResult(true, successMessage, destinationPath, descriptors));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Plug-in import cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import plug-in package from {Source}.", sourcePath);
            return Task.FromResult(new PluginImportResult(false, "Import failed. Check logs for details.", null, Array.Empty<IServiceDescriptor>()));
        }
    }

    private static bool PathsEqual(string first, string second)
    {
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return string.Equals(Path.GetFullPath(first), Path.GetFullPath(second), comparison);
    }
}
