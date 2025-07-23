using System;
using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private readonly MainViewModel _viewModel;

        public MainView(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            _viewModel.EditRequested += OnEditRequested;
            ContentFrame.Content = new HomePage { DataContext = _viewModel };
        }

        private void AddService_Click(object sender, RoutedEventArgs e)
        {
            // Open the service creation workflow in its own window
            var window = App.AppHost.Services.GetRequiredService<CreateServiceWindow>();

            if (window.ShowDialog() == true)
            {
                var name = window.CreatedServiceName;
                var type = window.CreatedServiceType;

                var newService = new ServiceViewModel
                {
                    DisplayName = $"{type} - {name}",
                    ServiceType = type,
                    IsActive = false
                };

                newService.SetColorsByType();

                newService.ServicePage = type switch
                {
                    "TCP" => new TcpServiceView(
                        App.AppHost.Services.GetRequiredService<TcpServiceViewModel>(),
                        App.AppHost.Services.GetRequiredService<IStartupService>()),
                    "HTTP" => App.AppHost.Services.GetRequiredService<HttpServiceView>(),
                    "File Observer" => App.AppHost.Services.GetRequiredService<FileObserverView>(),
                    "HID" => new HidViews(),
                    "Heartbeat" => new HeartbeatView(App.AppHost.Services.GetRequiredService<HeartbeatViewModel>()),
                    _ => null
                };

                _viewModel.Services.Add(newService);
                _viewModel.SelectedService = newService;

                if (newService.ServicePage != null)
                {
                    ContentFrame.Content = newService.ServicePage;
                }
            }
        }

        private void OnEditRequested(ServiceViewModel service)
        {
            if (service.ServicePage != null)
            {
                ContentFrame.Content = service.ServicePage;
            }
        }

        private void RemoveService_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm &&
                vm.RemoveServiceCommand.CanExecute(null))
            {
                vm.RemoveServiceCommand.Execute(null);
            }
        }
        private void EditService_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedService?.Page != null)
            {
                _viewModel.SelectedService.IsActive = false;
                ContentFrame.Navigate(_viewModel.SelectedService.Page);
            }
        }

        private void ServiceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel.SelectedService?.Page != null)
            {
                ContentFrame.Navigate(_viewModel.SelectedService.Page);
            }
        }

        private void OpenCsvViewer_Click(object sender, RoutedEventArgs e)
        {
            var vm = App.AppHost.Services.GetRequiredService<CsvViewerViewModel>();
            var window = new CsvViewerWindow(vm);
            vm.RequestClose += () => window.Close();
            window.ShowDialog();
        }

    }
}
