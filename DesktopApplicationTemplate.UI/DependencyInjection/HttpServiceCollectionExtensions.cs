using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Http;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Http;
using DesktopApplicationTemplate.UI.ViewModels.Http.Advanced;
using DesktopApplicationTemplate.UI.ViewModels.Http.Create;
using DesktopApplicationTemplate.UI.ViewModels.Http.Edit;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Views.Http;
using DesktopApplicationTemplate.UI.Views.Http.Advanced;
using DesktopApplicationTemplate.UI.Views.Http.Create;
using DesktopApplicationTemplate.UI.Views.Http.Edit;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.UI.DependencyInjection
{
    public static class HttpServiceCollectionExtensions
    {
        public static IServiceCollection AddHttpUi(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<HttpServiceView>();
            services.AddTransient<HttpServiceViewModel>();
            services.AddTransient<HttpCreateServiceView>();
            services.AddTransient<HttpCreateServiceViewModel>();
            services.AddTransient<ServiceCreateViewModelBase<HttpServiceOptions>, HttpCreateServiceViewModel>();
            services.AddTransient<HttpEditServiceView>();
            services.AddTransient<HttpEditServiceViewModel>();
            services.AddTransient<ServiceEditViewModelBase<HttpServiceOptions>, HttpEditServiceViewModel>();
            services.AddTransient<HttpAdvancedConfigView>();
            services.AddTransient<HttpAdvancedConfigViewModel>();

            services.AddSingleton<ServiceUiRegistration<ServiceListModel, Page>>(sp => CreateHttpRegistration(sp.GetRequiredService<IServiceCatalog>()));

            return services;
        }

        private static ServiceUiRegistration<ServiceListModel, Page> CreateHttpRegistration(IServiceCatalog catalog)
        {
            var descriptor = ServiceRegistrationHelper.GetDescriptorOrThrow(catalog, ServiceDescriptorIds.Http);

            return new ServiceUiRegistration<ServiceListModel, Page>(
                descriptor.Id,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<HttpServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var routing = provider.GetRequiredService<IMessageRoutingService>();
                    var svc = new ServiceListModel(routing)
                    {
                        DescriptorId = descriptor.Id,
                        DisplayName = ctx.Name,
                        Type = ServiceType.Http,
                        IsActive = false
                    };

                    svc.SetOptions(ctx.Options ?? new HttpServiceOptions(), descriptor.Id);

                    mainView.GetOrCreateServicePage(svc);
                    return svc;
                },
                provider => provider.GetRequiredService<HttpServiceView>(),
                (provider, defaultName) =>
                {
                    var vm = provider.GetRequiredService<HttpCreateServiceViewModel>();
                    vm.ServiceName = defaultName;
                    var mainView = provider.GetRequiredService<MainView>();
                    vm.ServiceSaved += (name, options) =>
                    {
                        ServiceRegistrationHelper.QueueServiceAddition(mainView, ServiceType.Http, name, (HttpServiceOptions)options);
                    };
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    var view = ActivatorUtilities.CreateInstance<HttpCreateServiceView>(provider, vm);
                    vm.AdvancedConfigRequested += opts =>
                    {
                        var advVm = ActivatorUtilities.CreateInstance<HttpAdvancedConfigViewModel>(provider, opts);
                        var advView = provider.GetRequiredService<HttpAdvancedConfigView>();
                        advView.Initialize(advVm);
                        advVm.Saved += _ => mainView.ShowPage(view);
                        advVm.BackRequested += () => mainView.ShowPage(view);
                        mainView.ShowPage(advView);
                    };
                    return view;
                },
                ApplyPresentation: static (service, metadata) => service.ApplyPresentation(metadata));
        }
    }
}
