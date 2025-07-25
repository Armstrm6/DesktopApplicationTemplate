using System.Windows;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class CloseConfirmationWindow : Window
    {
        public bool DontShowAgain { get; private set; }

        public CloseConfirmationWindow()
        {
            InitializeComponent();
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            DontShowAgain = DontShowAgainCheckBox.IsChecked == true;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
