using System;
using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Mqtt.UI.EditHandlers;
using DesktopApplicationTemplate.Services.Mqtt.UI.Factories;
using DesktopApplicationTemplate.Services.Mqtt.UI.Navigation;
using DesktopApplicationTemplate.Services.Mqtt.UI.Services;
using DesktopApplicationTemplate.Services.Mqtt.UI.ViewModels.Mqtt;
using DesktopApplicationTemplate.Services.Mqtt.UI.ViewModels.Mqtt.Advanced;
using DesktopApplicationTemplate.Services.Mqtt.UI.ViewModels.Mqtt.Create;
using DesktopApplicationTemplate.Services.Mqtt.UI.ViewModels.Mqtt.Edit;
using DesktopApplicationTemplate.Services.Mqtt.UI.Views.Mqtt;
using DesktopApplicationTemplate.Services.Mqtt.UI.Views.Mqtt.Advanced;
using DesktopApplicationTemplate.Services.Mqtt.UI.Views.Mqtt.Create;
using DesktopApplicationTemplate.Services.Mqtt.UI.Views.Mqtt.Edit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Services.Mqtt.UI.Modules;

public sealed class MqttUiModule : IServiceModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services
            .AddOptions<MqttServiceOptions>()
            .BindConfiguration("MqttService");

        services.AddSingleton<MqttService>();
        services.AddTransient<MqttServiceFactory>();
        services.AddTransient<MqttNavigationHandler>();
        services.AddTransient<MqttEditServiceHandler>();

        services.AddTransient<MqttServiceViewModel>();
        services.AddTransient<MqttCreateServiceViewModel>();
        services.AddTransient<MqttEditServiceViewModel>();
        services.AddTransient<MqttEditConnectionViewModel>();
        services.AddTransient<MqttAdvancedConfigViewModel>();
        services.AddTransient<MqttTagSubscriptionsViewModel>();

        services.AddTransient<MqttCreateServiceView>();
        services.AddTransient<MqttEditServiceView>();
        services.AddTransient<MqttEditConnectionView>();
        services.AddTransient<MqttAdvancedConfigView>();
        services.AddTransient<MqttTagSubscriptionsView>();
    }

    public IEnumerable<IServiceDescriptor> DescribeServices() => Array.Empty<IServiceDescriptor>();
}
