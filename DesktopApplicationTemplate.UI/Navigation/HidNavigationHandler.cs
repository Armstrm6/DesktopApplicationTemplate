using System;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.Factories;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Navigation
{
    public class HidNavigationHandler : INavigationHandler
    {
        private readonly IServiceProvider _services;
        private readonly MainView _mainView;

        public ServiceType ServiceType => ServiceType.Hid;

        public HidNavigationHandler(IServiceProvider services, MainView mainView)
        {
            _services = services;
            _mainView = mainView;
        }

        public Page CreateView(string defaultName)
        {
            var vm = _services.GetRequiredService<HidCreateServiceViewModel>();
            vm.ServiceName = defaultName;
            vm.ServiceSaved += (name, options) => _ = _mainView.AddServiceAsync(ServiceType, new ServiceFactoryOptions<HidServiceOptions>(name, options));
            vm.EditCancelled += _mainView.ShowCreateServiceSelectionPage;
            var view = ActivatorUtilities.CreateInstance<HidCreateServiceView>(_services, vm);
            vm.AdvancedConfigRequested += opts =>
            {
                var advVm = ActivatorUtilities.CreateInstance<HidAdvancedConfigViewModel>(_services, opts);
                var advView = _services.GetRequiredService<HidAdvancedConfigView>();
                advView.Initialize(advVm);
                advVm.Saved += _ => _mainView.ShowPage(view);
                advVm.BackRequested += () => _mainView.ShowPage(view);
                _mainView.ShowPage(advView);
            };
            return view;
        }
    }
}
