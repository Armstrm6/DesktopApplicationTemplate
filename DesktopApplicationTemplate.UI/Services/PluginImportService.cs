using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

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

        var moduleDiagnostics = new List<string>();

        try
        {
            var fileName = Path.GetFileName(sourcePath);
            var destinationPath = _options.GetPackagePath(fileName);

            if (!PluginPackageUtilities.PathsEqual(sourcePath, destinationPath))
            {
                _logger.LogInformation("Copying plug-in package {Source} to {Destination}.", sourcePath, destinationPath);
                File.Copy(sourcePath, destinationPath, overwrite: true);
            }

            var loaderLogger = _loggerFactory.CreateLogger<PluginLoader>();
            var loader = new PluginLoader(_options, loaderLogger);
            IReadOnlyCollection<Assembly> pluginAssemblies = loader.LoadPluginAssemblies();

            var modules = ServiceModuleDiscovery.InstantiateModules(
                pluginAssemblies,
                (assembly, errors) =>
                {
                    var errorArray = errors?.Where(static error => error is not null).Cast<Exception>().ToArray()
                        ?? Array.Empty<Exception>();
                    if (errorArray.Length == 0)
                    {
                        return;
                    }

                    var diagnostic = FormatTypeLoadDiagnostic(assembly, errorArray);
                    moduleDiagnostics.Add(diagnostic);

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
            var descriptors = ServiceModuleDiscovery.DescribeServices(modules);

            if (descriptors.Count == 0)
            {
                var diagnosticSnapshot = moduleDiagnostics.ToArray();
                var message = moduleDiagnostics.Count > 0
                    ? "The package did not expose any service descriptors. Some modules were skipped due to load errors."
                    : "The package did not expose any service descriptors.";
                _logger.LogWarning(message);
                return Task.FromResult(new PluginImportResult(false, message, destinationPath, Array.Empty<IServiceDescriptor>())
                {
                    Diagnostics = diagnosticSnapshot,
                });
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

            var mergedDescriptors = merged.Values.ToArray();
            var uiRegistrations = ExtractUiRegistrations(modules, mergedDescriptors);

            _catalog.UpdateDescriptors(mergedDescriptors);

            var diagnosticMessages = moduleDiagnostics.ToArray();
            var successMessage = descriptors.Count == 1
                ? "Imported 1 service descriptor."
                : $"Imported {descriptors.Count} service descriptors.";
            if (diagnosticMessages.Length > 0)
            {
                successMessage += " Some modules were skipped due to load errors.";
            }
            _logger.LogInformation("{Message} Package: {PackagePath}.", successMessage, destinationPath);

            return Task.FromResult(new PluginImportResult(true, successMessage, destinationPath, descriptors)
            {
                ImportedUiRegistrations = uiRegistrations,
                Diagnostics = diagnosticMessages,
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Plug-in import cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import plug-in package from {Source}.", sourcePath);
            return Task.FromResult(new PluginImportResult(false, "Import failed. Check logs for details.", null, Array.Empty<IServiceDescriptor>())
            {
                Diagnostics = moduleDiagnostics.ToArray(),
            });
        }
    }

    private IReadOnlyCollection<ServiceUiRegistration<ServiceListModel, Page>> ExtractUiRegistrations(
        IEnumerable<IServiceModule> modules,
        IEnumerable<IServiceDescriptor> descriptors)
    {
        try
        {
            var descriptorSnapshot = descriptors?.ToArray() ?? Array.Empty<IServiceDescriptor>();
            var catalog = new ServiceCatalog(descriptorSnapshot);
            var services = new ServiceCollection();
            services.AddSingleton(catalog);
            services.AddSingleton<IServiceCatalog>(catalog);
            ServiceModuleDiscovery.RegisterModules(modules, services);

            using var provider = services.BuildServiceProvider();
            return provider.GetServices<ServiceUiRegistration<ServiceListModel, Page>>().ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to extract UI registrations from plug-in modules.");
            return Array.Empty<ServiceUiRegistration<ServiceListModel, Page>>();
        }
    }

    private static string FormatTypeLoadDiagnostic(Assembly? assembly, IReadOnlyCollection<Exception> errors)
    {
        var assemblyName = assembly?.GetName().Name ?? assembly?.FullName ?? "(unknown)";
        var summary = string.Join("; ", errors.Select(error => error.Message));
        return FormattableString.Invariant($"Assembly '{assemblyName}' skipped: {summary}");
    }
}
