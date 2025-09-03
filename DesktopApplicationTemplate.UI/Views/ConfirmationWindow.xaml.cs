using System.Windows;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class ConfirmationWindow : Window
    {
        public bool DontShowAgain { get; private set; }

        public ConfirmationWindow(string messageText, string positiveButtonText, string? negativeButtonText = null)
        {
            InitializeComponent();

            MessageTextBlock.Text = messageText;
            PositiveButton.Content = positiveButtonText;

            if (string.IsNullOrWhiteSpace(negativeButtonText))
            {
                NegativeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                NegativeButton.Content = negativeButtonText;
            }
        }

        private void Positive_Click(object sender, RoutedEventArgs e)
        {
            DontShowAgain = DontShowAgainCheckBox.IsChecked == true;
            DialogResult = true;
        }

        private void Negative_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
