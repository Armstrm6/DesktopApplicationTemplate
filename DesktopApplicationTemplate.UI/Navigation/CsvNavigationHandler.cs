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
    public class CsvNavigationHandler : INavigationHandler
    {
        private readonly IServiceProvider _services;
        private readonly MainView _mainView;

        public ServiceType ServiceType => ServiceType.Csv;

        public CsvNavigationHandler(IServiceProvider services, MainView mainView)
        {
            _services = services;
            _mainView = mainView;
        }

        public Page CreateView(string defaultName)
        {
            var vm = _services.GetRequiredService<CsvServiceEditorViewModel>();
            vm.ServiceName = defaultName;
            vm.ServiceSaved += (name, options) => _ = _mainView.AddServiceAsync(ServiceType, new ServiceFactoryOptions<CsvServiceOptions>(name, options));
            vm.EditCancelled += _mainView.ShowCreateServiceSelectionPage;
            var view = _services.GetRequiredService<CsvServiceEditorView>();
            view.Initialize(vm);
            vm.AdvancedConfigRequested += opts =>
            {
                var advVm = ActivatorUtilities.CreateInstance<CsvAdvancedConfigViewModel>(_services, opts);
                var advView = _services.GetRequiredService<CsvAdvancedConfigView>();
                advView.Initialize(advVm);
                advVm.Saved += _ => _mainView.ShowPage(view);
                advVm.BackRequested += () => _mainView.ShowPage(view);
                _mainView.ShowPage(advView);
            };
            return view;
        }
    }
}
