using System;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt.Edit;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Views.Mqtt;
using DesktopApplicationTemplate.UI.Views.Mqtt.Edit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Factories
{
    public class MqttServiceFactory : IServiceFactory
    {
        private readonly IServiceProvider _services;
        private readonly Func<MainView> _getMainView;
        private readonly MainViewModel _mainViewModel;

        public ServiceType ServiceType => ServiceType.Mqtt;

        public MqttServiceFactory(IServiceProvider services, Func<MainView> getMainView, MainViewModel mainViewModel)
        {
            _services = services;
            _getMainView = getMainView;
            _mainViewModel = mainViewModel;
        }

        public ServiceListModel Create(object optionsObj)
        {
            var ctx = (ServiceFactoryOptions<MqttServiceOptions>)optionsObj;
            var name = ctx.Name;
            var options = ctx.Options;

            var newService = new ServiceListModel
            {
                DisplayName = $"MQTT - {name}",
                Type = ServiceType.Mqtt,
                IsActive = false
            };

            _getMainView().GetOrCreateServicePage(newService);

            var opt = _services.GetRequiredService<IOptions<MqttServiceOptions>>().Value;
            opt.Host = options.Host;
            opt.Port = options.Port;
            opt.ClientId = options.ClientId;
            opt.Username = options.Username;
            opt.Password = options.Password;
            opt.ConnectionType = options.ConnectionType;
            opt.WillTopic = options.WillTopic;
            opt.WillPayload = options.WillPayload;
            opt.WillQualityOfService = options.WillQualityOfService;
            opt.WillRetain = options.WillRetain;
            opt.KeepAliveSeconds = options.KeepAliveSeconds;
            opt.CleanSession = options.CleanSession;
            opt.ReconnectDelay = options.ReconnectDelay;

            if (newService.ServicePage is MqttTagSubscriptionsView mqttView)
            {
                var mqttVm = (MqttTagSubscriptionsViewModel)mqttView.DataContext!;
                newService.ActiveChanged += active =>
                {
                    if (active)
                    {
                        _ = mqttVm.ConnectAsync();
                    }
                };
                mqttVm.EditConnectionRequested += (_, _) =>
                {
                    var editView = _services.GetRequiredService<MqttEditConnectionView>();
                    if (editView.DataContext is MqttEditConnectionViewModel vm)
                    {
                        var opt = _services.GetRequiredService<IOptions<MqttServiceOptions>>().Value;
                        vm.Load(opt);
                        vm.HighlightMissingFields();
                        vm.RequestClose += (_, _) =>
                        {
                            if (newService.ServicePage != null)
                                _getMainView().ShowPage(newService.ServicePage);
                            _ = _mainViewModel.SaveServicesAsync();
                        };
                    }
                    _getMainView().ShowPage(editView);
                };
            }

            return newService;
        }
    }
}
