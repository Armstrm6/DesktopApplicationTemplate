using System;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt.Advanced;
using DesktopApplicationTemplate.UI.Views.Mqtt;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DesktopApplicationTemplate.UI.EditHandlers;

public class MqttEditServiceHandler : IEditServiceHandler
{
    private readonly MainView _view;
    private readonly MainViewModel _viewModel;
    private readonly IServiceProvider _services;
    private readonly ILogger<MqttEditServiceHandler>? _logger;

    public MqttEditServiceHandler(MainView view, MainViewModel viewModel, IServiceProvider services, ILogger<MqttEditServiceHandler>? logger = null)
    {
        _view = view;
        _viewModel = viewModel;
        _services = services;
        _logger = logger;
    }

    public void Edit(ServiceListModel service)
    {
        var tagPage = _view.GetOrCreateServicePage(service);
        var options = _services.GetRequiredService<IOptions<MqttServiceOptions>>().Value;
        var vm = ActivatorUtilities.CreateInstance<MqttEditServiceViewModel>(_services, service.DisplayName.Split(" - ").Last(), options);
        var editView = _services.GetRequiredService<MqttEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            service.DisplayName = $"MQTT - {name}";
            if (tagPage != null)
                _view.ShowPage(tagPage);
            _ = _viewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (tagPage != null)
                _view.ShowPage(tagPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<MqttAdvancedConfigViewModel>(_services, opts);
            var advView = _services.GetRequiredService<MqttAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => _view.ShowPage(editView);
            advVm.BackRequested += () => _view.ShowPage(editView);
            _view.ShowPage(advView);
        };
        _view.ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }
}

