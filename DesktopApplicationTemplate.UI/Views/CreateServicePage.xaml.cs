using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class CreateServicePage : Page
    {
        private readonly CreateServiceViewModel _viewModel;
        public string CreatedServiceName { get; private set; } = string.Empty;
        public string CreatedServiceType { get; private set; } = string.Empty;
        public event Action<string,string>? ServiceCreated;
        public event Action? Cancelled;

        public CreateServicePage(CreateServiceViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void ServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not string type)
                return;

            var vm = (CreateServiceViewModel)DataContext;
            vm.SelectedServiceType = type;
            CreatedServiceName = vm.ServiceName;
            CreatedServiceType = vm.SelectedServiceType;
            ServiceCreated?.Invoke(CreatedServiceName, CreatedServiceType);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Cancelled?.Invoke();
        }
    }
}
