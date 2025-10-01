using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Navigation;
using DesktopApplicationTemplate.UI.Factories;

namespace DesktopApplicationTemplate.Tests;

public static class TestHelpers
{
    public static ServiceListModel CreateService(ServiceType type, string name) => new ServiceListModel
    {
        Type = type,
        DescriptorId = type.ToDescriptorId(),
        DisplayName = $"{type.ToLegacyString()} - {name}"
    };

    public static MainViewModel CreateMainViewModel(
        CsvService csvService,
        NetworkConfigurationViewModel networkConfig,
        INetworkConfigurationService networkService,
        IDictionary<ServiceType, IEditServiceHandler>? editHandlers = null,
        ILoggingService? loggingService = null,
        string? servicesFilePath = null,
        IPluginImportService? pluginImportService = null,
        FakeServiceCatalog? catalog = null,
        IServiceUiRegistry? registry = null)
    {
        catalog ??= FakeServiceCatalog.CreateWithAllLegacyTypes();
        var editHandlerFactories = new Dictionary<string, Func<IEditServiceHandler>>(StringComparer.Ordinal);
        if (editHandlers != null)
        {
            foreach (var kvp in editHandlers)
            {
                var descriptorId = kvp.Key.ToDescriptorId();
                editHandlerFactories[descriptorId] = () => kvp.Value;
            }
        }

        registry ??= new FakeServiceUiRegistry(editHandlers: editHandlerFactories);
        var fileDialog = new StubFileDialogService();
        var importService = pluginImportService ?? new NullPluginImportService();

        return new MainViewModel(
            csvService,
            networkConfig,
            networkService,
            registry,
            catalog,
            fileDialog,
            importService,
            loggingService,
            servicesFilePath);
    }

    private sealed class NullPluginImportService : IPluginImportService
    {
        public Task<PluginImportResult> ImportAsync(string sourcePath, CancellationToken cancellationToken = default)
            => Task.FromResult(new PluginImportResult(false, "Import not available in tests.", null, Array.Empty<IServiceDescriptor>()));
    }
}
