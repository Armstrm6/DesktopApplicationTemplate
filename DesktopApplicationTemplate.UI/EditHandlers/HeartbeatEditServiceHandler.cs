using System;
using System.Linq;
using DesktopApplicationTemplate.Core.Services.Protocols.Heartbeat;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Heartbeat.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Heartbeat.Advanced;
using DesktopApplicationTemplate.UI.Views.Heartbeat.Edit;
using DesktopApplicationTemplate.UI.Views.Heartbeat.Advanced;
using DesktopApplicationTemplate.UI.Services;
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
        var hbPage = mainView.GetOrCreateServicePage(service);
        var options = service.GetOptions<HeartbeatServiceOptions>() ?? new HeartbeatServiceOptions();
        service.SetOptions(options);
        var vm = ActivatorUtilities.CreateInstance<HeartbeatEditServiceViewModel>(_services, service.DisplayName, options);
        var editView = _services.GetRequiredService<HeartbeatEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += async (name, opts) =>
        {
            var trimmed = string.IsNullOrWhiteSpace(name)
                ? mainViewModel.GenerateServiceName(service.Type)
                : name.Trim();
            if (mainViewModel.Services.Any(s => s != service && s.DisplayName.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
            {
                trimmed = mainViewModel.GenerateServiceName(service.Type);
            }

            var previousName = service.DisplayName;
            if (!string.Equals(previousName, trimmed, StringComparison.Ordinal))
            {
                mainViewModel.ClearRoutingCache(service.Type, previousName);
                mainViewModel.ClearRoutingCache(service.Type, trimmed);
            }

            service.DisplayName = trimmed;
            service.SetOptions(opts);
            if (hbPage != null)
                mainView.ShowPage(hbPage);
            await mainViewModel.SaveServicesAsync().ConfigureAwait(false);
        };
        vm.EditCancelled += () =>
        {
            if (hbPage != null)
                mainView.ShowPage(hbPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<HeartbeatAdvancedConfigViewModel>(_services, opts);
            var advView = _services.GetRequiredService<HeartbeatAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => mainView.ShowPage(editView);
            advVm.BackRequested += () => mainView.ShowPage(editView);
            mainView.ShowPage(advView);
        };
        mainView.ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }
}

