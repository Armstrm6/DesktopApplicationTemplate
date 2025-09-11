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
    public class TcpNavigationHandler : INavigationHandler
    {
        private readonly IServiceProvider _services;
        private readonly MainView _mainView;

        public ServiceType ServiceType => ServiceType.Tcp;

        public TcpNavigationHandler(IServiceProvider services, MainView mainView)
        {
            _services = services;
            _mainView = mainView;
        }

        public Page CreateView(string defaultName)
        {
            var vm = _services.GetRequiredService<TcpCreateServiceViewModel>();
            vm.ServiceName = defaultName;
            vm.ServiceSaved += (name, options) => _ = _mainView.AddServiceAsync(ServiceType, new ServiceFactoryOptions<TcpServiceOptions>(name, options));
            vm.EditCancelled += _mainView.ShowCreateServiceSelectionPage;
            var view = ActivatorUtilities.CreateInstance<TcpCreateServiceView>(_services, vm);
            return view;
        }
    }
}
