using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services.Csv.Options;
using DesktopApplicationTemplate.Services.Csv.UI.ViewModels.Csv.Advanced;
using DesktopApplicationTemplate.Services.Csv.UI.ViewModels.Csv.Edit;
using DesktopApplicationTemplate.Services.Csv.UI.Views.Csv.Advanced;
using DesktopApplicationTemplate.Services.Csv.UI.Views.Csv.Edit;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.Services.Csv.UI.EditHandlers;

public class CsvEditServiceHandler : IEditServiceHandler
{
    private readonly Func<MainView> _getMainView;
    private readonly Func<MainViewModel> _getMainViewModel;
    private readonly IServiceProvider _services;
    private readonly ILogger<CsvEditServiceHandler>? _logger;

    public CsvEditServiceHandler(Func<MainView> getMainView, Func<MainViewModel> getMainViewModel, IServiceProvider services, ILogger<CsvEditServiceHandler>? logger = null)
    {
        _getMainView = getMainView;
        _getMainViewModel = getMainViewModel;
        _services = services;
        _logger = logger;
    }

    public void Edit(ServiceListModel service)
    {
        var mainView = _getMainView();
        var mainViewModel = _getMainViewModel();
        var csvPage = mainView.GetOrCreateServicePage(service);
        var payload = service.GetPayload<CsvServiceOptions>();
        if (payload is null)
        {
            payload = new CsvServiceOptions();
            service.SetPayload(payload);
        }

        var options = payload;
        var viewModel = _services.GetRequiredService<CsvServiceEditorViewModel>();
        viewModel.Load(service.DisplayName.Split(" - ").Last(), options);
        var editView = _services.GetRequiredService<CsvServiceEditorView>();
        editView.Initialize(viewModel);
        viewModel.ServiceSaved += (name, opts) =>
        {
            var catalog = _services.GetRequiredService<IServiceCatalog>();
            var descriptor = ResolveDescriptor(catalog, service.DescriptorId, service.Type);
            service.ApplyDescriptor(descriptor, name);
            service.SetPayload(opts);
            if (csvPage != null)
            {
                mainView.ShowPage(csvPage);
            }

            _ = mainViewModel.SaveServicesAsync();
        };
        viewModel.EditCancelled += () =>
        {
            if (csvPage != null)
            {
                mainView.ShowPage(csvPage);
            }
        };
        viewModel.AdvancedConfigRequested += opts =>
        {
            var advancedViewModel = ActivatorUtilities.CreateInstance<CsvAdvancedConfigViewModel>(_services, opts);
            var advancedView = _services.GetRequiredService<CsvAdvancedConfigView>();
            advancedView.Initialize(advancedViewModel);
            advancedViewModel.Saved += _ => mainView.ShowPage(editView);
            advancedViewModel.BackRequested += () => mainView.ShowPage(editView);
            mainView.ShowPage(advancedView);
        };
        mainView.ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }

    private static IServiceDescriptor? ResolveDescriptor(IServiceCatalog catalog, string descriptorId, ServiceType serviceType)
    {
        if (!string.IsNullOrWhiteSpace(descriptorId) && catalog.TryGetById(descriptorId, out var byId))
        {
            return byId;
        }

        if (catalog.TryGetByLegacyType(serviceType, out var legacy))
        {
            return legacy;
        }

        if (catalog.LegacyMap.TryGetValue(serviceType, out var fallbackId) && catalog.TryGetById(fallbackId, out var fallback))
        {
            return fallback;
        }

        return null;
    }
}
