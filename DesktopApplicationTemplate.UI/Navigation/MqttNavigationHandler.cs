using DesktopApplicationTemplate.UI.Views;
using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.Factories;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt.Create;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt.Advanced;
using DesktopApplicationTemplate.UI.Views.Mqtt.Create;
using DesktopApplicationTemplate.UI.Views.Mqtt.Advanced;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Common.Descriptors;
using DesktopApplicationTemplate.UI.Configuration;

namespace DesktopApplicationTemplate.UI.Navigation
{
    [ServiceDescriptorRegistration(MqttServiceDescriptor.DescriptorId, ServiceRegistrationKind.NavigationHandler)]
    public class MqttNavigationHandler : INavigationHandler
    {
        private readonly IServiceProvider _services;
        private readonly Func<MainView> _getMainView;
        private readonly IServiceCatalog _catalog;

        public string DescriptorId => MqttServiceDescriptor.DescriptorId;

        public MqttNavigationHandler(IServiceProvider services, Func<MainView> getMainView, IServiceCatalog catalog)
        {
            _services = services;
            _getMainView = getMainView;
            _catalog = catalog;
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
            _catalog.TryGetById(DescriptorId, out var descriptor);
            var context = new ServiceFactoryContext(DescriptorId, name, options, descriptor);
            return mainView.AddServiceAsync(context);
        }

    }
}
