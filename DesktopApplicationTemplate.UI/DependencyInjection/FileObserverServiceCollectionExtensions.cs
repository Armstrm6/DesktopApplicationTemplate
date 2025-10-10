using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.FileObserver;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.UI.EditHandlers;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.FileObserver;
using DesktopApplicationTemplate.UI.ViewModels.FileObserver.Advanced;
using DesktopApplicationTemplate.UI.ViewModels.FileObserver.Create;
using DesktopApplicationTemplate.UI.ViewModels.FileObserver.Edit;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Views.FileObserver;
using DesktopApplicationTemplate.UI.Views.FileObserver.Advanced;
using DesktopApplicationTemplate.UI.Views.FileObserver.Create;
using DesktopApplicationTemplate.UI.Views.FileObserver.Edit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.DependencyInjection
{
    public static class FileObserverServiceCollectionExtensions
    {
        public static IServiceCollection AddFileObserverUi(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<FileObserverView>();
            services.AddTransient<FileObserverViewModel>();
            services.AddTransient<FileObserverCreateServiceView>();
            services.AddTransient<FileObserverCreateServiceViewModel>();
            services.AddTransient<ServiceCreateViewModelBase<FileObserverServiceOptions>, FileObserverCreateServiceViewModel>();
            services.AddTransient<FileObserverEditServiceView>();
            services.AddTransient<FileObserverEditServiceViewModel>();
            services.AddTransient<ServiceEditViewModelBase<FileObserverServiceOptions>, FileObserverEditServiceViewModel>();
            services.AddTransient<FileObserverAdvancedConfigView>();
            services.AddTransient<FileObserverAdvancedConfigViewModel>();

            services.AddSingleton<ServiceUiRegistration<ServiceListModel, Page>>(sp => CreateFileObserverRegistration(sp.GetRequiredService<IServiceCatalog>()));

            return services;
        }

        private static ServiceUiRegistration<ServiceListModel, Page> CreateFileObserverRegistration(IServiceCatalog catalog)
        {
            var descriptor = ServiceRegistrationHelper.GetDescriptorOrThrow(catalog, ServiceDescriptorIds.FileObserver);

            return new ServiceUiRegistration<ServiceListModel, Page>(
                descriptor.Id,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<FileObserverServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var routing = provider.GetRequiredService<IMessageRoutingService>();
                    var svc = new ServiceListModel(routing)
                    {
                        DescriptorId = descriptor.Id,
                        DisplayName = ctx.Name,
                        Type = ServiceType.FileObserver,
                        IsActive = false
                    };

                    svc.SetOptions(ctx.Options ?? new FileObserverServiceOptions(), descriptor.Id);

                    mainView.GetOrCreateServicePage(svc);
                    return svc;
                },
                provider => provider.GetRequiredService<FileObserverView>(),
                (provider, defaultName) =>
                {
                    var vm = provider.GetRequiredService<FileObserverCreateServiceViewModel>();
                    vm.ServiceName = defaultName;
                    var mainView = provider.GetRequiredService<MainView>();
                    vm.ServiceSaved += (name, options) =>
                    {
                        ServiceRegistrationHelper.QueueServiceAddition(mainView, ServiceType.FileObserver, name, (FileObserverServiceOptions)options);
                    };
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    var view = ActivatorUtilities.CreateInstance<FileObserverCreateServiceView>(provider, vm);
                    vm.AdvancedConfigRequested += opts =>
                    {
                        var advVm = ActivatorUtilities.CreateInstance<FileObserverAdvancedConfigViewModel>(provider, opts);
                        var advView = provider.GetRequiredService<FileObserverAdvancedConfigView>();
                        advView.Initialize(advVm);
                        advVm.Saved += _ => mainView.ShowPage(view);
                        advVm.BackRequested += () => mainView.ShowPage(view);
                        mainView.ShowPage(advView);
                    };
                    return view;
                },
                ApplyPresentation: static (service, metadata) => service.ApplyPresentation(metadata),
                CreateEditHandler: provider => new FileObserverEditServiceHandler(
                    () => provider.GetRequiredService<MainView>(),
                    () => provider.GetRequiredService<MainViewModel>(),
                    provider,
                    provider.GetService<ILogger<FileObserverEditServiceHandler>>()));
        }
    }
}
