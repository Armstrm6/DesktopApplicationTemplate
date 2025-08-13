using System.Windows;
using Xceed.Wpf.Toolkit;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class ColorPickerWindow : Window
    {
        public ColorPickerWindow()
        {
            InitializeComponent();
        }

        public System.Windows.Media.Color ChosenColor => ColorCanvas.SelectedColor ?? System.Windows.Media.Colors.Transparent;

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
