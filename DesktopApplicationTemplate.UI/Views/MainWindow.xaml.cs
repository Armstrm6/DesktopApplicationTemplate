using DesktopApplicationTemplate.UI.ViewModels;
using System.Windows;
using DesktopApplicationTemplate.UI.Views;

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
            var window = new CreateServiceWindow();
            if (window.ShowDialog() == true)
            {
                var newService = new ServiceViewModel
                {
                    DisplayName = $"{window.CreatedServiceType} - {window.CreatedServiceName}",
                    IsActive = false
                };

                Services.Add(newService);
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
