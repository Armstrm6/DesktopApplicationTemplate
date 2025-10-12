using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Hid;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.UI.EditHandlers;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Hid;
using DesktopApplicationTemplate.UI.ViewModels.Hid.Advanced;
using DesktopApplicationTemplate.UI.ViewModels.Hid.Create;
using DesktopApplicationTemplate.UI.ViewModels.Hid.Edit;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Views.Hid;
using DesktopApplicationTemplate.UI.Views.Hid.Advanced;
using DesktopApplicationTemplate.UI.Views.Hid.Create;
using DesktopApplicationTemplate.UI.Views.Hid.Edit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.DependencyInjection
{
    public static class HidServiceCollectionExtensions
    {
        public static IServiceCollection AddHidUi(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<HidViewModel>();
            services.AddTransient<HidView>();
            services.AddTransient<HidCreateServiceView>();
            services.AddTransient<HidCreateServiceViewModel>();
            services.AddTransient<ServiceCreateViewModelBase<HidServiceOptions>, HidCreateServiceViewModel>();
            services.AddTransient<HidEditServiceView>();
            services.AddTransient<HidEditServiceViewModel>();
            services.AddTransient<ServiceEditViewModelBase<HidServiceOptions>, HidEditServiceViewModel>();
            services.AddTransient<HidAdvancedConfigView>();
            services.AddTransient<HidAdvancedConfigViewModel>();

            services.AddSingleton<ServiceUiRegistration<ServiceListModel, Page>>(sp => CreateHidRegistration(sp.GetRequiredService<IServiceCatalog>()));

            return services;
        }

        private static ServiceUiRegistration<ServiceListModel, Page> CreateHidRegistration(IServiceCatalog catalog)
        {
            var descriptor = ServiceRegistrationHelper.GetDescriptorOrThrow(catalog, ServiceDescriptorIds.Hid);

            return new ServiceUiRegistration<ServiceListModel, Page>(
                descriptor.Id,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<HidServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var routing = provider.GetRequiredService<IMessageRoutingService>();
                    var svc = new ServiceListModel(routing)
                    {
                        DescriptorId = descriptor.Id,
                        DisplayName = ctx.Name,
                        Type = ServiceType.Hid,
                        IsActive = false
                    };

                    svc.SetOptions(ctx.Options ?? new HidServiceOptions(), descriptor.Id);

                    mainView.GetOrCreateServicePage(svc);
                    return svc;
                },
                provider => provider.GetRequiredService<HidView>(),
                (provider, defaultName) =>
                {
                    var vm = provider.GetRequiredService<HidCreateServiceViewModel>();
                    vm.ServiceName = defaultName;
                    var mainView = provider.GetRequiredService<MainView>();
                    vm.ServiceSaved += (name, options) =>
                    {
                        ServiceRegistrationHelper.QueueServiceAddition(mainView, ServiceType.Hid, name, (HidServiceOptions)options);
                        return Task.CompletedTask;
                    };
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    var view = ActivatorUtilities.CreateInstance<HidCreateServiceView>(provider, vm);
                    vm.AdvancedConfigRequested += opts =>
                    {
                        var advVm = ActivatorUtilities.CreateInstance<HidAdvancedConfigViewModel>(provider, opts);
                        var advView = provider.GetRequiredService<HidAdvancedConfigView>();
                        advView.Initialize(advVm);
                        advVm.Saved += _ => mainView.ShowPage(view);
                        advVm.BackRequested += () => mainView.ShowPage(view);
                        mainView.ShowPage(advView);
                    };
                    return view;
                },
                ApplyPresentation: static (service, metadata) => service.ApplyPresentation(metadata),
                CreateEditHandler: provider => new HidEditServiceHandler(
                    () => provider.GetRequiredService<MainView>(),
                    () => provider.GetRequiredService<MainViewModel>(),
                    provider,
                    provider.GetService<ILogger<HidEditServiceHandler>>()));
        }
    }
}
