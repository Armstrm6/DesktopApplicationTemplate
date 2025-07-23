using System.Windows;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class CsvViewerWindow : Window
    {
        public CsvViewerWindow(CsvViewerViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
