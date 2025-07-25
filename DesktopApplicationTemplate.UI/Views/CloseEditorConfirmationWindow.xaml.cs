using System.Windows;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class CloseEditorConfirmationWindow : Window
    {
        public bool DontShowAgain { get; private set; }

        public CloseEditorConfirmationWindow()
        {
            InitializeComponent();
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            DontShowAgain = DontShowAgainCheckBox.IsChecked == true;
            DialogResult = true;
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
