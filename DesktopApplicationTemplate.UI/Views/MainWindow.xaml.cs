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
        }

        private void AddService_Click(object sender, RoutedEventArgs e)
        {
            var window = App.AppHost.Services.GetRequiredService<CreateServiceWindow>();
            if (window.ShowDialog() == true)
            {
                var newService = new ServiceViewModel
                {
                    DisplayName = $"{window.CreatedServiceType} - {window.CreatedServiceName}",
                    IsActive = false
                };

                _viewModel.Services.Add(newService);
                _viewModel.SelectedService = newService;

                System.Windows.Controls.Page? page = window.CreatedServiceType switch
                {
                    "TCP" => new TcpServiceView(App.AppHost.Services.GetRequiredService<TcpServiceViewModel>(), App.AppHost.Services.GetRequiredService<Services.IStartupService>()),
                    "HTTP" => App.AppHost.Services.GetRequiredService<HttpServiceView>(),
                    "File Observer" => App.AppHost.Services.GetRequiredService<FileObserverView>(),
                    "HID" => new HidViews(),
                    _ => null
                };

                if (page != null)
                {
                    var navWindow = new Window
                    {
                        Title = newService.DisplayName,
                        Content = page,
                        Width = 800,
                        Height = 450
                    };
                    navWindow.Show();
                }
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
