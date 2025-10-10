using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Tcp;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.UI.EditHandlers;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Tcp;
using DesktopApplicationTemplate.UI.ViewModels.Tcp.Create;
using DesktopApplicationTemplate.UI.ViewModels.Tcp.Edit;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Views.Tcp;
using DesktopApplicationTemplate.UI.Views.Tcp.Create;
using DesktopApplicationTemplate.UI.Views.Tcp.Edit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.DependencyInjection
{
    public static class TcpServiceCollectionExtensions
    {
        public static IServiceCollection AddTcpUi(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<ServiceMessageTableViewModel>();
            services.AddTransient<Func<ServiceMessageTableViewModel>>(sp => () => sp.GetRequiredService<ServiceMessageTableViewModel>());
            services.AddTransient<TcpServiceMessagesView>();
            services.AddTransient<TcpServiceMessagesViewModel>();
            services.AddTransient<TcpCreateServiceView>();
            services.AddTransient<TcpCreateServiceViewModel>();
            services.AddTransient<ServiceCreateViewModelBase<TcpServiceOptions>, TcpCreateServiceViewModel>();
            services.AddTransient<TcpEditServiceView>();
            services.AddTransient<TcpEditServiceViewModel>();
            services.AddTransient<ServiceEditViewModelBase<TcpServiceOptions>, TcpEditServiceViewModel>();

            services.AddSingleton<ServiceUiRegistration<ServiceListModel, Page>>(sp => CreateTcpRegistration(sp.GetRequiredService<IServiceCatalog>()));

            return services;
        }

        private static ServiceUiRegistration<ServiceListModel, Page> CreateTcpRegistration(IServiceCatalog catalog)
        {
            var descriptor = ServiceRegistrationHelper.GetDescriptorOrThrow(catalog, ServiceDescriptorIds.Tcp);

            return new ServiceUiRegistration<ServiceListModel, Page>(
                descriptor.Id,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<TcpServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var routing = provider.GetRequiredService<IMessageRoutingService>();
                    var svc = new ServiceListModel(routing)
                    {
                        DescriptorId = descriptor.Id,
                        DisplayName = ctx.Name,
                        Type = ServiceType.Tcp,
                        IsActive = false
                    };

                    svc.SetOptions(ctx.Options ?? new TcpServiceOptions(), descriptor.Id);

                    mainView.GetOrCreateServicePage(svc);
                    return svc;
                },
                provider => provider.GetRequiredService<TcpServiceMessagesView>(),
                (provider, defaultName) =>
                {
                    var vm = provider.GetRequiredService<TcpCreateServiceViewModel>();
                    vm.ServiceName = defaultName;
                    var mainView = provider.GetRequiredService<MainView>();
                    vm.ServiceSaved += (name, options) =>
                    {
                        ServiceRegistrationHelper.QueueServiceAddition(mainView, ServiceType.Tcp, name, (TcpServiceOptions)options);
                        return Task.CompletedTask;
                    };
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    return ActivatorUtilities.CreateInstance<TcpCreateServiceView>(provider, vm);
                },
                ApplyPresentation: static (service, metadata) => service.ApplyPresentation(metadata),
                CreateEditHandler: provider => new TcpEditServiceHandler(
                    () => provider.GetRequiredService<MainView>(),
                    () => provider.GetRequiredService<MainViewModel>(),
                    provider,
                    provider.GetService<ILogger<TcpEditServiceHandler>>()));
        }
    }
}
