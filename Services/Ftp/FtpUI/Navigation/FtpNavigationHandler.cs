using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Ftp.Descriptors;
using DesktopApplicationTemplate.Services.Ftp.UI.ViewModels.Ftp.Advanced;
using DesktopApplicationTemplate.Services.Ftp.UI.ViewModels.Ftp.Create;
using DesktopApplicationTemplate.Services.Ftp.UI.Views.Ftp.Advanced;
using DesktopApplicationTemplate.Services.Ftp.UI.Views.Ftp.Create;
using DesktopApplicationTemplate.UI.Configuration;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Services.Ftp.UI.Navigation
{
    [ServiceDescriptorRegistration(FtpServiceDescriptor.DescriptorId, ServiceRegistrationKind.NavigationHandler)]
    public class FtpNavigationHandler : INavigationHandler
    {
        private readonly IServiceProvider _services;
        private readonly Func<MainView> _getMainView;
        private readonly IServiceCatalog _catalog;

        public string DescriptorId => FtpServiceDescriptor.DescriptorId;

        public FtpNavigationHandler(IServiceProvider services, Func<MainView> getMainView, IServiceCatalog catalog)
        {
            _services = services;
            _getMainView = getMainView;
            _catalog = catalog;
        }

        public Page CreateView(string defaultName)
        {
            var vm = _services.GetRequiredService<FtpServerCreateViewModel>();
            vm.ServiceName = defaultName;
            var mainView = _getMainView();
            vm.ServiceSaved += (name, options) => _ = AddServiceAsync(name, options);
            vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
            var view = ActivatorUtilities.CreateInstance<FtpServerCreateView>(_services, vm);
            vm.AdvancedConfigRequested += opts =>
            {
                var advVm = ActivatorUtilities.CreateInstance<FtpServerAdvancedConfigViewModel>(_services, opts);
                var advView = _services.GetRequiredService<FtpServerAdvancedConfigView>();
                advView.Initialize(advVm);
                advVm.Saved += _ => mainView.ShowPage(view);
                advVm.BackRequested += () => mainView.ShowPage(view);
                mainView.ShowPage(advView);
            };
            return view;
        }

        public Task AddServiceAsync(string name, object options)
        {
            var mainView = _getMainView();
            _catalog.TryGetById(DescriptorId, out var descriptor);
            var context = new ServiceFactoryContext(DescriptorId, name, options, descriptor);
            return mainView.AddServiceAsync(context);
        }
    }
}
