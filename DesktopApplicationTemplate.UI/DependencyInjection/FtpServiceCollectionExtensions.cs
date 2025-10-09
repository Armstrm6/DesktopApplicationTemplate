using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Ftp;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Ftp;
using DesktopApplicationTemplate.UI.ViewModels.Ftp.Advanced;
using DesktopApplicationTemplate.UI.ViewModels.Ftp.Create;
using DesktopApplicationTemplate.UI.ViewModels.Ftp.Edit;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Views.Ftp;
using DesktopApplicationTemplate.UI.Views.Ftp.Advanced;
using DesktopApplicationTemplate.UI.Views.Ftp.Create;
using DesktopApplicationTemplate.UI.Views.Ftp.Edit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DesktopApplicationTemplate.UI.DependencyInjection
{
    public static class FtpServiceCollectionExtensions
    {
        public static IServiceCollection AddFtpUi(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<FTPServiceView>();
            services.AddTransient<FtpServiceViewModel>();
            services.AddTransient<FtpServerCreateView>();
            services.AddTransient<FtpServerCreateViewModel>();
            services.AddTransient<ServiceCreateViewModelBase<CoreFtpServerOptions>, FtpServerCreateViewModel>();
            services.AddTransient<FtpServerAdvancedConfigView>();
            services.AddTransient<FtpServerAdvancedConfigViewModel>();
            services.AddTransient<FtpServerEditView>();
            services.AddTransient<FtpServerEditViewModel>();
            services.AddTransient<ServiceEditViewModelBase<CoreFtpServerOptions>, FtpServerEditViewModel>();

            services.TryAddEnumerable(ServiceDescriptor.Singleton<ServiceUiRegistration<ServiceListModel, Page>>(sp => CreateFtpRegistration(sp.GetRequiredService<IServiceCatalog>())));

            return services;
        }

        private static ServiceUiRegistration<ServiceListModel, Page> CreateFtpRegistration(IServiceCatalog catalog)
        {
            var descriptor = ServiceRegistrationHelper.GetDescriptorOrThrow(catalog, ServiceDescriptorIds.Ftp);

            return new ServiceUiRegistration<ServiceListModel, Page>(
                descriptor.Id,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<CoreFtpServerOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var ftpOptions = ctx.Options ?? new CoreFtpServerOptions();
                    var svc = new ServiceListModel
                    {
                        DescriptorId = descriptor.Id,
                        DisplayName = ctx.Name,
                        Type = ServiceType.Ftp,
                        IsActive = false
                    };

                    svc.SetOptions(ftpOptions, descriptor.Id);

                    mainView.GetOrCreateServicePage(svc);

                    return svc;
                },
                provider => provider.GetRequiredService<FTPServiceView>(),
                (provider, defaultName) =>
                {
                    var vm = provider.GetRequiredService<FtpServerCreateViewModel>();
                    vm.ServiceName = defaultName;
                    var mainView = provider.GetRequiredService<MainView>();
                    vm.ServiceSaved += (name, options) =>
                    {
                        ServiceRegistrationHelper.QueueServiceAddition(mainView, ServiceType.Ftp, name, (CoreFtpServerOptions)options);
                    };
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    var view = ActivatorUtilities.CreateInstance<FtpServerCreateView>(provider, vm);
                    vm.AdvancedConfigRequested += opts =>
                    {
                        var advVm = ActivatorUtilities.CreateInstance<FtpServerAdvancedConfigViewModel>(provider, opts);
                        var advView = provider.GetRequiredService<FtpServerAdvancedConfigView>();
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
