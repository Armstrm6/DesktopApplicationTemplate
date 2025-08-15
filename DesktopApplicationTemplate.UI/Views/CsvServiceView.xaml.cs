using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views
{
    public partial class CsvServiceView : Page
    {
        private readonly CsvViewerViewModel _viewModel;

        public CsvServiceView(CsvViewerViewModel vm)
        {
            InitializeComponent();
            _viewModel = vm;
            DataContext = vm;
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            var help = new AsciiHelpWindow();
            help.ShowDialog();
        }
    }
}
