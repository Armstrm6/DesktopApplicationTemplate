using DesktopApplicationTemplate.UI.Views;
using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.Factories;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels.Scp.Create;
using DesktopApplicationTemplate.UI.ViewModels.Scp.Advanced;
using DesktopApplicationTemplate.UI.Views.Scp;
using Microsoft.Extensions.DependencyInjection;
using DesktopApplicationTemplate.Models;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Navigation
{
    public class ScpNavigationHandler : INavigationHandler
    {
        private readonly IServiceProvider _services;
        private readonly Func<MainView> _getMainView;

        public ServiceType ServiceType => ServiceType.Scp;

        public ScpNavigationHandler(IServiceProvider services, Func<MainView> getMainView)
        {
            _services = services;
            _getMainView = getMainView;
        }

        public Page CreateView(string defaultName)
        {
            var vm = _services.GetRequiredService<ScpCreateServiceViewModel>();
            vm.ServiceName = defaultName;
            var mainView = _getMainView();
            vm.ServiceSaved += (name, options) => _ = AddServiceAsync(name, options);
            vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
            var view = ActivatorUtilities.CreateInstance<ScpCreateServiceView>(_services, vm);
            vm.AdvancedConfigRequested += opts =>
            {
                var advVm = ActivatorUtilities.CreateInstance<ScpAdvancedConfigViewModel>(_services, opts);
                var advView = _services.GetRequiredService<ScpAdvancedConfigView>();
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
            return mainView.AddServiceAsync(ServiceType, new ServiceFactoryOptions<ScpServiceOptions>(name, (ScpServiceOptions)options));
        }
    }
}
