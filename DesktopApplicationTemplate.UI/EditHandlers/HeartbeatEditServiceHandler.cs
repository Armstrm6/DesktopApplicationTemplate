using System;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Heartbeat.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Heartbeat.Advanced;
using DesktopApplicationTemplate.UI.Views.Heartbeat;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.EditHandlers;

public class HeartbeatEditServiceHandler : IEditServiceHandler
{
    private readonly MainView _view;
    private readonly MainViewModel _viewModel;
    private readonly IServiceProvider _services;
    private readonly ILogger<HeartbeatEditServiceHandler>? _logger;

    public HeartbeatEditServiceHandler(MainView view, MainViewModel viewModel, IServiceProvider services, ILogger<HeartbeatEditServiceHandler>? logger = null)
    {
        _view = view;
        _viewModel = viewModel;
        _services = services;
        _logger = logger;
    }

    public void Edit(ServiceListModel service)
    {
        var hbPage = _view.GetOrCreateServicePage(service);
        var options = service.HeartbeatOptions ?? new HeartbeatServiceOptions();
        var vm = ActivatorUtilities.CreateInstance<HeartbeatEditServiceViewModel>(_services, service.DisplayName.Split(" - ").Last(), options);
        var editView = _services.GetRequiredService<HeartbeatEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            service.DisplayName = $"Heartbeat - {name}";
            service.HeartbeatOptions = opts;
            if (hbPage != null)
                _view.ShowPage(hbPage);
            _ = _viewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (hbPage != null)
                _view.ShowPage(hbPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<HeartbeatAdvancedConfigViewModel>(_services, opts);
            var advView = _services.GetRequiredService<HeartbeatAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => _view.ShowPage(editView);
            advVm.BackRequested += () => _view.ShowPage(editView);
            _view.ShowPage(advView);
        };
        _view.ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }
}

