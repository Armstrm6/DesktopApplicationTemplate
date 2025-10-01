using System;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Http.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Http.Advanced;
using DesktopApplicationTemplate.UI.Views.Http.Edit;
using DesktopApplicationTemplate.UI.Views.Http.Advanced;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.EditHandlers;

public class HttpEditServiceHandler : IEditServiceHandler
{
    private readonly Func<MainView> _getMainView;
    private readonly Func<MainViewModel> _getMainViewModel;
    private readonly IServiceProvider _services;
    private readonly ILogger<HttpEditServiceHandler>? _logger;

    public HttpEditServiceHandler(Func<MainView> getMainView, Func<MainViewModel> getMainViewModel, IServiceProvider services, ILogger<HttpEditServiceHandler>? logger = null)
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
        var httpPage = mainView.GetOrCreateServicePage(service);
        var payload = service.GetPayload<HttpServiceOptions>();
        if (payload is null)
        {
            payload = new HttpServiceOptions();
            service.SetPayload(payload);
        }

        var options = payload;
        var vm = ActivatorUtilities.CreateInstance<HttpEditServiceViewModel>(_services, service.DisplayName.Split(" - ").Last(), options);
        var editView = _services.GetRequiredService<HttpEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            var catalog = _services.GetRequiredService<IServiceCatalog>();
            var descriptor = ResolveDescriptor(catalog, service.DescriptorId, service.Type);
            service.ApplyDescriptor(descriptor, name);
            service.SetPayload(opts);
            if (httpPage != null)
                mainView.ShowPage(httpPage);
            _ = mainViewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (httpPage != null)
                mainView.ShowPage(httpPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<HttpAdvancedConfigViewModel>(_services, opts);
            var advView = _services.GetRequiredService<HttpAdvancedConfigView>();
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

