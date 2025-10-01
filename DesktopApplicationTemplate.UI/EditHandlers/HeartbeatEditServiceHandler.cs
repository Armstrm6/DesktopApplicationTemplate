using System;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Heartbeat.Edit;
using DesktopApplicationTemplate.UI.Views.Heartbeat.Edit;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.EditHandlers;

public class HeartbeatEditServiceHandler : IEditServiceHandler
{
    private readonly Func<MainView> _getMainView;
    private readonly Func<MainViewModel> _getMainViewModel;
    private readonly IServiceProvider _services;
    private readonly ILogger<HeartbeatEditServiceHandler>? _logger;

    public HeartbeatEditServiceHandler(Func<MainView> getMainView, Func<MainViewModel> getMainViewModel, IServiceProvider services, ILogger<HeartbeatEditServiceHandler>? logger = null)
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
        var heartbeatPage = mainView.GetOrCreateServicePage(service);
        var payload = service.GetPayload<HeartbeatServiceOptions>();
        if (payload is null)
        {
            payload = new HeartbeatServiceOptions();
            service.SetPayload(payload);
        }

        var vm = ActivatorUtilities.CreateInstance<HeartbeatEditServiceViewModel>(_services, service.DisplayName.Split(" - ").Last(), payload);
        var editView = _services.GetRequiredService<HeartbeatEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            var catalog = _services.GetRequiredService<IServiceCatalog>();
            var descriptor = ResolveDescriptor(catalog, service.DescriptorId, service.Type);
            service.ApplyDescriptor(descriptor, name);
            service.SetPayload(opts);
            if (heartbeatPage != null)
            {
                mainView.ShowPage(heartbeatPage);
            }

            _ = mainViewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (heartbeatPage != null)
            {
                mainView.ShowPage(heartbeatPage);
            }
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
