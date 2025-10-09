using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt.Create;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt.Edit;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Views.Mqtt;
using DesktopApplicationTemplate.UI.Views.Mqtt.Create;
using DesktopApplicationTemplate.UI.Views.Mqtt.Edit;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.UI.DependencyInjection
{
    public static class MqttServiceCollectionExtensions
    {
        public static IServiceCollection AddMqttUi(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<MqttCreateServiceView>();
            services.AddTransient<MqttCreateServiceViewModel>();
            services.AddTransient<ServiceCreateViewModelBase<MqttServiceOptions>, MqttCreateServiceViewModel>();
            services.AddTransient<MqttEditServiceView>();
            services.AddTransient<MqttEditServiceViewModel>();
            services.AddTransient<ServiceEditViewModelBase<MqttServiceOptions>, MqttEditServiceViewModel>();
            services.AddTransient<MqttEditConnectionView>();
            services.AddTransient<MqttTagSubscriptionsView>();

            services.AddSingleton<ServiceUiRegistration<ServiceListModel, Page>>(sp => CreateMqttRegistration(sp.GetRequiredService<IServiceCatalog>()));

            return services;
        }

        private static ServiceUiRegistration<ServiceListModel, Page> CreateMqttRegistration(IServiceCatalog catalog)
        {
            var descriptor = ServiceRegistrationHelper.GetDescriptorOrThrow(catalog, ServiceDescriptorIds.Mqtt);

            return new ServiceUiRegistration<ServiceListModel, Page>(
                descriptor.Id,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<MqttServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var newService = new ServiceListModel
                    {
                        DescriptorId = descriptor.Id,
                        DisplayName = ctx.Name,
                        Type = ServiceType.Mqtt,
                        IsActive = false
                    };

                    newService.SetOptions(ctx.Options ?? new MqttServiceOptions(), descriptor.Id);
                    mainView.GetOrCreateServicePage(newService);

                    return newService;
                },
                provider => provider.GetRequiredService<MqttTagSubscriptionsView>(),
                (provider, defaultName) =>
                {
                    var vm = provider.GetRequiredService<MqttCreateServiceViewModel>();
                    vm.ServiceName = defaultName;
                    var mainView = provider.GetRequiredService<MainView>();
                    vm.ServiceSaved += (name, options) =>
                    {
                        ServiceRegistrationHelper.QueueServiceAddition(mainView, ServiceType.Mqtt, name, (MqttServiceOptions)options);
                    };
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    var view = ActivatorUtilities.CreateInstance<MqttCreateServiceView>(provider, vm);
                    return view;
                },
                ApplyPresentation: static (service, metadata) => service.ApplyPresentation(metadata));
        }
    }
}
