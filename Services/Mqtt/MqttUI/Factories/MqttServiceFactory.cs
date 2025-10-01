using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services.Mqtt.Descriptors;
using DesktopApplicationTemplate.Services.Mqtt.UI.Services;
using DesktopApplicationTemplate.Services.Mqtt.UI.ViewModels.Mqtt;
using DesktopApplicationTemplate.Services.Mqtt.UI.ViewModels.Mqtt.Edit;
using DesktopApplicationTemplate.Services.Mqtt.UI.Views.Mqtt;
using DesktopApplicationTemplate.Services.Mqtt.UI.Views.Mqtt.Edit;
using DesktopApplicationTemplate.UI.Factories;
using DesktopApplicationTemplate.UI.Configuration;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DesktopApplicationTemplate.Services.Mqtt.UI.Factories
{
    [ServiceDescriptorRegistration(MqttServiceDescriptor.DescriptorId, ServiceRegistrationKind.ServiceFactory)]
    public class MqttServiceFactory : IServiceFactory
    {
        private readonly IServiceProvider _services;
        private readonly Func<MainView> _getMainView;
        private readonly MainViewModel _mainViewModel;
        private readonly IServiceCatalog _catalog;

        public ServiceType ServiceType => ServiceType.Mqtt;

        public MqttServiceFactory(IServiceProvider services, Func<MainView> getMainView, MainViewModel mainViewModel, IServiceCatalog catalog)
        {
            _services = services;
            _getMainView = getMainView;
            _mainViewModel = mainViewModel;
            _catalog = catalog;
        }

        public ServiceListModel Create(ServiceFactoryContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var descriptor = _catalog.TryGetById(context.DescriptorId, out var resolved)
                ? resolved
                : context.Descriptor;

            var options = context.GetPayload<MqttServiceOptions>() ?? new MqttServiceOptions();

            var newService = new ServiceListModel
            {
                Type = ServiceType.Mqtt,
                DescriptorId = context.DescriptorId,
                IsActive = false
            };

            newService.SetPayload(options);

            newService.ApplyDescriptor(descriptor, context.ServiceName);

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
