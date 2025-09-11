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
    public class HttpNavigationHandler : INavigationHandler
    {
        private readonly IServiceProvider _services;
        private readonly MainView _mainView;

        public ServiceType ServiceType => ServiceType.Http;

        public HttpNavigationHandler(IServiceProvider services, MainView mainView)
        {
            _services = services;
            _mainView = mainView;
        }

        public Page CreateView(string defaultName)
        {
            var vm = _services.GetRequiredService<HttpCreateServiceViewModel>();
            vm.ServiceName = defaultName;
            vm.ServiceSaved += (name, options) => _ = _mainView.AddServiceAsync(ServiceType, new ServiceFactoryOptions<HttpServiceOptions>(name, options));
            vm.EditCancelled += _mainView.ShowCreateServiceSelectionPage;
            var view = ActivatorUtilities.CreateInstance<HttpCreateServiceView>(_services, vm);
            vm.AdvancedConfigRequested += opts =>
            {
                var advVm = ActivatorUtilities.CreateInstance<HttpAdvancedConfigViewModel>(_services, opts);
                var advView = _services.GetRequiredService<HttpAdvancedConfigView>();
                advView.Initialize(advVm);
                advVm.Saved += _ => _mainView.ShowPage(view);
                advVm.BackRequested += () => _mainView.ShowPage(view);
                _mainView.ShowPage(advView);
            };
            return view;
        }
    }
}
