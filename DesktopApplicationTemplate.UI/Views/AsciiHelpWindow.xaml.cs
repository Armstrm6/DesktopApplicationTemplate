using System.Windows;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class AsciiHelpWindow : Window
    {
        public AsciiHelpWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
