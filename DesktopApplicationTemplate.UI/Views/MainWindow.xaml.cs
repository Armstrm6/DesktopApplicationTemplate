using DesktopApplicationTemplate.UI.ViewModels;
using System.Windows;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate;
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
            var page = App.AppHost.Services.GetRequiredService<CreateServicePage>();
            page.ServiceCreated += (name, type) =>
            {
                var newService = new ServiceViewModel
                {
                    DisplayName = $"{type} - {name}",
                    IsActive = false
                };

                newService.ServicePage = type switch
                {
                    "TCP" => new TcpServiceView(App.AppHost.Services.GetRequiredService<TcpServiceViewModel>(), App.AppHost.Services.GetRequiredService<Services.IStartupService>()),
                    "HTTP" => App.AppHost.Services.GetRequiredService<HttpServiceView>(),
                    "File Observer" => App.AppHost.Services.GetRequiredService<FileObserverView>(),
                    "HID" => new HidViews(),
                    _ => null
                };

                _viewModel.Services.Add(newService);
                _viewModel.SelectedService = newService;

                if (newService.ServicePage != null)
                    ContentFrame.Content = newService.ServicePage;
            };

            ContentFrame.Content = page;
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
    }
}
