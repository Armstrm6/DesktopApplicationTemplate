using DesktopApplicationTemplate.UI.Views;
using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.Factories;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels.Hid.Create;
using DesktopApplicationTemplate.UI.ViewModels.Hid.Advanced;
using DesktopApplicationTemplate.UI.Views.Hid.Create;
using DesktopApplicationTemplate.UI.Views.Hid.Advanced;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Services.Common.Descriptors;
using DesktopApplicationTemplate.UI.Configuration;

namespace DesktopApplicationTemplate.UI.Navigation
{
    [ServiceDescriptorRegistration(HidServiceDescriptor.DescriptorId, ServiceRegistrationKind.NavigationHandler)]
    public class HidNavigationHandler : INavigationHandler
    {
        private readonly IServiceProvider _services;
        private readonly Func<MainView> _getMainView;

        public string DescriptorId => HidServiceDescriptor.DescriptorId;

        public HidNavigationHandler(IServiceProvider services, Func<MainView> getMainView)
        {
            _services = services;
            _getMainView = getMainView;
        }

        public Page CreateView(string defaultName)
        {
            var vm = _services.GetRequiredService<HidCreateServiceViewModel>();
            vm.ServiceName = defaultName;
            var mainView = _getMainView();
            vm.ServiceSaved += (name, options) => _ = AddServiceAsync(name, options);
            vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
            var view = ActivatorUtilities.CreateInstance<HidCreateServiceView>(_services, vm);
            vm.AdvancedConfigRequested += opts =>
            {
                var advVm = ActivatorUtilities.CreateInstance<HidAdvancedConfigViewModel>(_services, opts);
                var advView = _services.GetRequiredService<HidAdvancedConfigView>();
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
            return mainView.AddServiceAsync(DescriptorId, new ServiceFactoryOptions<HidServiceOptions>(name, (HidServiceOptions)options));
        }
    }
}
