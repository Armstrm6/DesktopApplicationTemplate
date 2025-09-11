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
    public class FtpNavigationHandler : INavigationHandler
    {
        private readonly IServiceProvider _services;
        private readonly MainView _mainView;

        public ServiceType ServiceType => ServiceType.Ftp;

        public FtpNavigationHandler(IServiceProvider services, MainView mainView)
        {
            _services = services;
            _mainView = mainView;
        }

        public Page CreateView(string defaultName)
        {
            var vm = _services.GetRequiredService<FtpServerCreateViewModel>();
            vm.ServiceName = defaultName;
            vm.ServiceSaved += (name, options) => _ = _mainView.AddServiceAsync(ServiceType, new ServiceFactoryOptions<FtpServerOptions>(name, options));
            vm.EditCancelled += _mainView.ShowCreateServiceSelectionPage;
            var view = ActivatorUtilities.CreateInstance<FtpServerCreateView>(_services, vm);
            vm.AdvancedConfigRequested += opts =>
            {
                var advVm = ActivatorUtilities.CreateInstance<FtpServerAdvancedConfigViewModel>(_services, opts);
                var advView = _services.GetRequiredService<FtpServerAdvancedConfigView>();
                advView.Initialize(advVm);
                advVm.Saved += _ => _mainView.ShowPage(view);
                advVm.BackRequested += () => _mainView.ShowPage(view);
                _mainView.ShowPage(advView);
            };
            return view;
        }
    }
}
