using System;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Scp.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Scp.Advanced;
using DesktopApplicationTemplate.UI.Views.Scp.Edit;
using DesktopApplicationTemplate.UI.Views.Scp.Advanced;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.EditHandlers;

public class ScpEditServiceHandler : IEditServiceHandler
{
    private readonly Func<MainView> _getMainView;
    private readonly Func<MainViewModel> _getMainViewModel;
    private readonly IServiceProvider _services;
    private readonly ILogger<ScpEditServiceHandler>? _logger;

    public ScpEditServiceHandler(Func<MainView> getMainView, Func<MainViewModel> getMainViewModel, IServiceProvider services, ILogger<ScpEditServiceHandler>? logger = null)
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
        var scpPage = mainView.GetOrCreateServicePage(service);
        var payload = service.GetPayload<ScpServiceOptions>();
        if (payload is null)
        {
            payload = new ScpServiceOptions();
            service.SetPayload(payload);
        }

        var vm = ActivatorUtilities.CreateInstance<ScpEditServiceViewModel>(_services, service.DisplayName.Split(" - ").Last(), payload);
        var editView = _services.GetRequiredService<ScpEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            var catalog = _services.GetRequiredService<IServiceCatalog>();
            var descriptor = ResolveDescriptor(catalog, service.DescriptorId, service.Type);
            service.ApplyDescriptor(descriptor, name);
            service.SetPayload(opts);
            if (scpPage != null)
            {
                mainView.ShowPage(scpPage);
            }

            _ = mainViewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (scpPage != null)
            {
                mainView.ShowPage(scpPage);
            }
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<ScpAdvancedConfigViewModel>(_services, opts);
            var advView = _services.GetRequiredService<ScpAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => mainView.ShowPage(editView);
            advVm.BackRequested += () => mainView.ShowPage(editView);
            mainView.ShowPage(advView);
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
