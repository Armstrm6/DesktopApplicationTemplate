using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Heartbeat;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.UI.EditHandlers;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Heartbeat;
using DesktopApplicationTemplate.UI.ViewModels.Heartbeat.Advanced;
using DesktopApplicationTemplate.UI.ViewModels.Heartbeat.Create;
using DesktopApplicationTemplate.UI.ViewModels.Heartbeat.Edit;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Views.Heartbeat;
using DesktopApplicationTemplate.UI.Views.Heartbeat.Advanced;
using DesktopApplicationTemplate.UI.Views.Heartbeat.Create;
using DesktopApplicationTemplate.UI.Views.Heartbeat.Edit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.DependencyInjection
{
    public static class HeartbeatServiceCollectionExtensions
    {
        public static IServiceCollection AddHeartbeatUi(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<HeartbeatView>();
            services.AddTransient<HeartbeatViewModel>();
            services.AddTransient<HeartbeatCreateServiceView>();
            services.AddTransient<HeartbeatCreateServiceViewModel>();
            services.AddTransient<ServiceCreateViewModelBase<HeartbeatServiceOptions>, HeartbeatCreateServiceViewModel>();
            services.AddTransient<HeartbeatEditServiceView>();
            services.AddTransient<HeartbeatEditServiceViewModel>();
            services.AddTransient<ServiceEditViewModelBase<HeartbeatServiceOptions>, HeartbeatEditServiceViewModel>();
            services.AddTransient<HeartbeatAdvancedConfigView>();
            services.AddTransient<HeartbeatAdvancedConfigViewModel>();

            services.AddSingleton<ServiceUiRegistration<ServiceListModel, Page>>(sp => CreateHeartbeatRegistration(sp.GetRequiredService<IServiceCatalog>()));

            return services;
        }

        private static ServiceUiRegistration<ServiceListModel, Page> CreateHeartbeatRegistration(IServiceCatalog catalog)
        {
            var descriptor = ServiceRegistrationHelper.GetDescriptorOrThrow(catalog, ServiceDescriptorIds.Heartbeat);

            return new ServiceUiRegistration<ServiceListModel, Page>(
                descriptor.Id,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<HeartbeatServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var routing = provider.GetRequiredService<IMessageRoutingService>();
                    var svc = new ServiceListModel(routing)
                    {
                        DescriptorId = descriptor.Id,
                        DisplayName = ctx.Name,
                        Type = ServiceType.Heartbeat,
                        IsActive = false
                    };

                    svc.SetOptions(ctx.Options ?? new HeartbeatServiceOptions(), descriptor.Id);

                    mainView.GetOrCreateServicePage(svc);
                    return svc;
                },
                provider => provider.GetRequiredService<HeartbeatView>(),
                (provider, defaultName) =>
                {
                    var vm = provider.GetRequiredService<HeartbeatCreateServiceViewModel>();
                    vm.ServiceName = defaultName;
                    var mainView = provider.GetRequiredService<MainView>();
                    vm.ServiceSaved += (name, options) =>
                    {
                        ServiceRegistrationHelper.QueueServiceAddition(mainView, ServiceType.Heartbeat, name, (HeartbeatServiceOptions)options);
                        return Task.CompletedTask;
                    };
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    var view = ActivatorUtilities.CreateInstance<HeartbeatCreateServiceView>(provider, vm);
                    vm.AdvancedConfigRequested += opts =>
                    {
                        var advVm = ActivatorUtilities.CreateInstance<HeartbeatAdvancedConfigViewModel>(provider, opts);
                        var advView = provider.GetRequiredService<HeartbeatAdvancedConfigView>();
                        advView.Initialize(advVm);
                        advVm.Saved += _ => mainView.ShowPage(view);
                        advVm.BackRequested += () => mainView.ShowPage(view);
                        mainView.ShowPage(advView);
                    };
                    return view;
                },
                ApplyPresentation: static (service, metadata) => service.ApplyPresentation(metadata),
                CreateEditHandler: provider => new HeartbeatEditServiceHandler(
                    () => provider.GetRequiredService<MainView>(),
                    () => provider.GetRequiredService<MainViewModel>(),
                    provider,
                    provider.GetService<ILogger<HeartbeatEditServiceHandler>>()));
        }
    }
}
