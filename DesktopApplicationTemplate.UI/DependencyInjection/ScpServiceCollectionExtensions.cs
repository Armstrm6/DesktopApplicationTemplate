using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Scp;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Scp;
using DesktopApplicationTemplate.UI.ViewModels.Scp.Advanced;
using DesktopApplicationTemplate.UI.ViewModels.Scp.Create;
using DesktopApplicationTemplate.UI.ViewModels.Scp.Edit;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Views.Scp;
using DesktopApplicationTemplate.UI.Views.Scp.Advanced;
using DesktopApplicationTemplate.UI.Views.Scp.Create;
using DesktopApplicationTemplate.UI.Views.Scp.Edit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DesktopApplicationTemplate.UI.DependencyInjection
{
    public static class ScpServiceCollectionExtensions
    {
        public static IServiceCollection AddScpUi(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<SCPServiceView>();
            services.AddTransient<ScpServiceViewModel>();
            services.AddTransient<ScpCreateServiceView>();
            services.AddTransient<ScpCreateServiceViewModel>();
            services.AddTransient<ServiceCreateViewModelBase<ScpServiceOptions>, ScpCreateServiceViewModel>();
            services.AddTransient<ScpEditServiceView>();
            services.AddTransient<ScpEditServiceViewModel>();
            services.AddTransient<ServiceEditViewModelBase<ScpServiceOptions>, ScpEditServiceViewModel>();
            services.AddTransient<ScpAdvancedConfigView>();
            services.AddTransient<ScpAdvancedConfigViewModel>();

            services.TryAddEnumerable(ServiceDescriptor.Singleton<ServiceUiRegistration<ServiceListModel, Page>>(sp => CreateScpRegistration(sp.GetRequiredService<IServiceCatalog>())));

            return services;
        }

        private static ServiceUiRegistration<ServiceListModel, Page> CreateScpRegistration(IServiceCatalog catalog)
        {
            var descriptor = ServiceRegistrationHelper.GetDescriptorOrThrow(catalog, ServiceDescriptorIds.Scp);

            return new ServiceUiRegistration<ServiceListModel, Page>(
                descriptor.Id,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<ScpServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var svc = new ServiceListModel
                    {
                        DescriptorId = descriptor.Id,
                        DisplayName = ctx.Name,
                        Type = ServiceType.Scp,
                        IsActive = false
                    };

                    svc.SetOptions(ctx.Options ?? new ScpServiceOptions(), descriptor.Id);

                    mainView.GetOrCreateServicePage(svc);
                    return svc;
                },
                provider => provider.GetRequiredService<SCPServiceView>(),
                (provider, defaultName) =>
                {
                    var vm = provider.GetRequiredService<ScpCreateServiceViewModel>();
                    vm.ServiceName = defaultName;
                    var mainView = provider.GetRequiredService<MainView>();
                    vm.ServiceSaved += (name, options) =>
                    {
                        ServiceRegistrationHelper.QueueServiceAddition(mainView, ServiceType.Scp, name, (ScpServiceOptions)options);
                    };
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    var view = ActivatorUtilities.CreateInstance<ScpCreateServiceView>(provider, vm);
                    vm.AdvancedConfigRequested += opts =>
                    {
                        var advVm = ActivatorUtilities.CreateInstance<ScpAdvancedConfigViewModel>(provider, opts);
                        var advView = provider.GetRequiredService<ScpAdvancedConfigView>();
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
