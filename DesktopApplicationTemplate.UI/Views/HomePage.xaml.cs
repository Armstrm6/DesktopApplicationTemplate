using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private void EditService_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.EditSelectedService();
            }
        }
    }
}
