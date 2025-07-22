using System.Windows;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class CreateServiceWindow : Window
    {
        public string CreatedServiceName { get; private set; }
        public string CreatedServiceType { get; private set; }

        public CreateServiceWindow()
        {
            InitializeComponent();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            var vm = (CreateServiceViewModel)DataContext;
            CreatedServiceName = vm.ServiceName;
            CreatedServiceType = vm.SelectedServiceType;

            if (string.IsNullOrWhiteSpace(CreatedServiceName) || string.IsNullOrWhiteSpace(CreatedServiceType))
            {
                MessageBox.Show("Please enter a name and select a type.", "Missing Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
