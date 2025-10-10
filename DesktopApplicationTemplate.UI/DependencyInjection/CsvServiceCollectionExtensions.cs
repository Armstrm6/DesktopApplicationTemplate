using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Csv;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.UI.EditHandlers;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Csv;
using DesktopApplicationTemplate.UI.ViewModels.Csv.Edit;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Views.Csv;
using DesktopApplicationTemplate.UI.Views.Csv.Edit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.UI.DependencyInjection
{
    public static class CsvServiceCollectionExtensions
    {
        public static IServiceCollection AddCsvUi(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<CsvViewerViewModel>();
            services.AddSingleton<ICsvOutput, FileCsvOutput>();
            services.AddTransient<CsvServiceAdapter>();
            services.AddTransient<CsvServiceView>();
            services.AddTransient<CsvServiceEditorView>();
            services.AddTransient<CsvServiceEditorViewModel>();
            services.AddTransient<ServiceEditorViewModelBase<CsvServiceOptions>, CsvServiceEditorViewModel>();

            services.AddSingleton<ServiceUiRegistration<ServiceListModel, Page>>(sp => CreateCsvRegistration(sp.GetRequiredService<IServiceCatalog>()));

            return services;
        }

        private static ServiceUiRegistration<ServiceListModel, Page> CreateCsvRegistration(IServiceCatalog catalog)
        {
            var descriptor = ServiceRegistrationHelper.GetDescriptorOrThrow(catalog, ServiceDescriptorIds.Csv);

            return new ServiceUiRegistration<ServiceListModel, Page>(
                descriptor.Id,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<CsvServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var routing = provider.GetRequiredService<IMessageRoutingService>();
                    var svc = new ServiceListModel(routing)
                    {
                        DescriptorId = descriptor.Id,
                        DisplayName = ctx.Name,
                        Type = ServiceType.Csv,
                        IsActive = false
                    };

                    svc.SetOptions(ctx.Options ?? new CsvServiceOptions(), descriptor.Id);

                    mainView.GetOrCreateServicePage(svc);
                    return svc;
                },
                provider => provider.GetRequiredService<CsvServiceView>(),
                (provider, defaultName) =>
                {
                    var vm = provider.GetRequiredService<CsvServiceEditorViewModel>();
                    vm.ServiceName = defaultName;
                    var mainView = provider.GetRequiredService<MainView>();
                    vm.ServiceSaved += (name, options) =>
                    {
                        ServiceRegistrationHelper.QueueServiceAddition(mainView, ServiceType.Csv, name, (CsvServiceOptions)options);
                    };
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    var view = provider.GetRequiredService<CsvServiceEditorView>();
                    view.Initialize(vm);
                    return view;
                },
                ApplyPresentation: static (service, metadata) => service.ApplyPresentation(metadata),
                CreateEditHandler: provider => new CsvEditServiceHandler(
                    () => provider.GetRequiredService<MainView>(),
                    () => provider.GetRequiredService<MainViewModel>(),
                    provider,
                    provider.GetService<ILogger<CsvEditServiceHandler>>()));
        }
    }
}
