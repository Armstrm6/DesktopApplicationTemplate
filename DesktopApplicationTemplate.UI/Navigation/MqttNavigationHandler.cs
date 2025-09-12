using DesktopApplicationTemplate.UI.Views;
using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.Factories;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt;
using DesktopApplicationTemplate.UI.Views.Mqtt;
using Microsoft.Extensions.DependencyInjection;
using DesktopApplicationTemplate.Models;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Navigation
{
    public class MqttNavigationHandler : INavigationHandler
    {
        private readonly IServiceProvider _services;
        private readonly Func<MainView> _getMainView;

        public ServiceType ServiceType => ServiceType.Mqtt;

        public MqttNavigationHandler(IServiceProvider services, Func<MainView> getMainView)
        {
            _services = services;
            _getMainView = getMainView;
        }

        public Page CreateView(string defaultName)
        {
            var vm = _services.GetRequiredService<MqttCreateServiceViewModel>();
            vm.ServiceName = defaultName;
            var mainView = _getMainView();
            vm.ServiceSaved += (name, options) => _ = AddServiceAsync(name, options);
            vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
            var view = ActivatorUtilities.CreateInstance<MqttCreateServiceView>(_services, vm);
            vm.AdvancedConfigRequested += opts =>
            {
                var advVm = ActivatorUtilities.CreateInstance<MqttAdvancedConfigViewModel>(_services, opts);
                var advView = _services.GetRequiredService<MqttAdvancedConfigView>();
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
            return mainView.AddServiceAsync(ServiceType, new ServiceFactoryOptions<MqttServiceOptions>(name, (MqttServiceOptions)options));
        }

    }
}
