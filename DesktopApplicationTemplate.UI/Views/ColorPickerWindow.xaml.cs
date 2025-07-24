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

        public System.Windows.Media.Color SelectedColor => (System.Windows.Media.Color)ColorCanvas.SelectedColor;

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
