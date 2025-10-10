using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels.Csv;

namespace DesktopApplicationTemplate.UI.Views.Csv
{
    public partial class CsvServiceView : Page
    {
        private readonly CsvServiceViewModel _viewModel;

        public CsvServiceView(CsvServiceViewModel vm)
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
