using System;
using System.Linq;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services.Mqtt.Descriptors;
using DesktopApplicationTemplate.Services.Mqtt.UI.Services;
using DesktopApplicationTemplate.Services.Mqtt.UI.ViewModels.Mqtt.Advanced;
using DesktopApplicationTemplate.Services.Mqtt.UI.ViewModels.Mqtt.Edit;
using DesktopApplicationTemplate.Services.Mqtt.UI.Views.Mqtt.Advanced;
using DesktopApplicationTemplate.Services.Mqtt.UI.Views.Mqtt.Edit;
using DesktopApplicationTemplate.UI.Configuration;
using DesktopApplicationTemplate.UI.EditHandlers;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DesktopApplicationTemplate.Services.Mqtt.UI.EditHandlers;

[ServiceDescriptorRegistration(MqttServiceDescriptor.DescriptorId, ServiceRegistrationKind.EditHandler)]
public class MqttEditServiceHandler : IEditServiceHandler
{
    private readonly Func<MainView> _getMainView;
    private readonly Func<MainViewModel> _getMainViewModel;
    private readonly IServiceProvider _services;
    private readonly ILogger<MqttEditServiceHandler>? _logger;

    public MqttEditServiceHandler(Func<MainView> getMainView, Func<MainViewModel> getMainViewModel, IServiceProvider services, ILogger<MqttEditServiceHandler>? logger = null)
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
        var tagPage = mainView.GetOrCreateServicePage(service);
        var options = _services.GetRequiredService<IOptions<MqttServiceOptions>>().Value;
        var vm = ActivatorUtilities.CreateInstance<MqttEditServiceViewModel>(_services, service.DisplayName.Split(" - ").Last(), options);
        var editView = _services.GetRequiredService<MqttEditServiceView>();
        editView.Initialize(vm);
        vm.ServiceSaved += (name, opts) =>
        {
            service.DisplayName = $"MQTT - {name}";
            if (tagPage != null)
                mainView.ShowPage(tagPage);
            _ = mainViewModel.SaveServicesAsync();
        };
        vm.EditCancelled += () =>
        {
            if (tagPage != null)
                mainView.ShowPage(tagPage);
        };
        vm.AdvancedConfigRequested += opts =>
        {
            var advVm = ActivatorUtilities.CreateInstance<MqttAdvancedConfigViewModel>(_services, opts);
            var advView = _services.GetRequiredService<MqttAdvancedConfigView>();
            advView.Initialize(advVm);
            advVm.Saved += _ => mainView.ShowPage(editView);
            advVm.BackRequested += () => mainView.ShowPage(editView);
            mainView.ShowPage(advView);
        };
        mainView.ShowPage(editView);
        _logger?.LogDebug("Edit workflow completed for {Name}", service.DisplayName);
    }
}

