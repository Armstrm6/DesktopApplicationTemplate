using System.Windows;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class SaveConfirmationWindow : Window
    {
        public bool DontShowAgain { get; private set; }

        public SaveConfirmationWindow()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DontShowAgain = DontShowAgainCheckBox.IsChecked == true;
            DialogResult = true;
        }
    }
}
