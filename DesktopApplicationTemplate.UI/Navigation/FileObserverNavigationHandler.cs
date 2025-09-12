using DesktopApplicationTemplate.UI.Views;
using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.Factories;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels.FileObserver;
using DesktopApplicationTemplate.UI.Views.FileObserver;
using Microsoft.Extensions.DependencyInjection;
using DesktopApplicationTemplate.Models;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Navigation
{
    public class FileObserverNavigationHandler : INavigationHandler
    {
        private readonly IServiceProvider _services;
        private readonly Func<MainView> _getMainView;

        public ServiceType ServiceType => ServiceType.FileObserver;

        public FileObserverNavigationHandler(IServiceProvider services, Func<MainView> getMainView)
        {
            _services = services;
            _getMainView = getMainView;
        }

        public Page CreateView(string defaultName)
        {
            var vm = _services.GetRequiredService<FileObserverCreateServiceViewModel>();
            vm.ServiceName = defaultName;
            var mainView = _getMainView();
            vm.ServiceSaved += (name, options) => _ = AddServiceAsync(name, options);
            vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
            var view = ActivatorUtilities.CreateInstance<FileObserverCreateServiceView>(_services, vm);
            vm.AdvancedConfigRequested += opts =>
            {
                var advVm = ActivatorUtilities.CreateInstance<FileObserverAdvancedConfigViewModel>(_services, opts);
                var advView = _services.GetRequiredService<FileObserverAdvancedConfigView>();
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
            return mainView.AddServiceAsync(ServiceType, new ServiceFactoryOptions<FileObserverServiceOptions>(name, (FileObserverServiceOptions)options));
        }
    }
}
