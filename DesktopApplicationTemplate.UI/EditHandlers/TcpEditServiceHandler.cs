using System;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Tcp.Edit;
using DesktopApplicationTemplate.UI.Views.Tcp.Edit;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.EditHandlers;

public class TcpEditServiceHandler : IEditServiceHandler
{
    private readonly Func<MainView> _getMainView;
    private readonly Func<MainViewModel> _getMainViewModel;
    private readonly IServiceProvider _services;
    private readonly ILogger<TcpEditServiceHandler>? _logger;

    public TcpEditServiceHandler(Func<MainView> getMainView, Func<MainViewModel> getMainViewModel, IServiceProvider services, ILogger<TcpEditServiceHandler>? logger = null)
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
        var tcpPage = mainView.GetOrCreateServicePage(service);
        var options = service.GetPayload<TcpServiceOptions>() ?? new TcpServiceOptions();
        var vm = _services.GetRequiredService<TcpEditServiceViewModel>();
        vm.ServiceType = service.Type;
        vm.Load(service.DisplayName.Split(" - ").Last(), options);
        var editView = _services.GetRequiredService<TcpEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            var catalog = _services.GetRequiredService<IServiceCatalog>();
            var descriptor = ResolveDescriptor(catalog, service.DescriptorId, vm.ServiceType);
            service.Type = descriptor?.LegacyType ?? vm.ServiceType;
            service.ApplyDescriptor(descriptor, name);

            service.SetPayload(opts);
            if (tcpPage != null)
                mainView.ShowPage(tcpPage);
            _ = mainViewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (tcpPage != null)
                mainView.ShowPage(tcpPage);
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

