using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class CreateServicePage : Page
    {
        private readonly CreateServiceViewModel _viewModel;
        public string CreatedServiceName { get; private set; }
        public string CreatedServiceType { get; private set; }
        public event Action<string,string>? ServiceCreated;
        public event Action? Cancelled;

        public CreateServicePage(CreateServiceViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            var vm = (CreateServiceViewModel)DataContext;
            CreatedServiceName = vm.ServiceName;
            CreatedServiceType = vm.SelectedServiceType;

            if (string.IsNullOrWhiteSpace(CreatedServiceName) || string.IsNullOrWhiteSpace(CreatedServiceType))
            {
                System.Windows.MessageBox.Show("Please enter a name and select a type.", "Missing Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ServiceCreated?.Invoke(CreatedServiceName, CreatedServiceType);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Cancelled?.Invoke();
        }
    }
}
