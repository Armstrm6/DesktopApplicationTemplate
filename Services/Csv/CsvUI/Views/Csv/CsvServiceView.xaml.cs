using System.Windows;
using System.Windows.Controls;
using DesktopApplicationTemplate.Services.Csv.UI.ViewModels.Csv;
using DesktopApplicationTemplate.UI.Views;

namespace DesktopApplicationTemplate.Services.Csv.UI.Views.Csv;

public partial class CsvServiceView : Page
{
    private readonly CsvViewerViewModel _viewModel;

    public CsvServiceView(CsvViewerViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    private void Help_Click(object sender, RoutedEventArgs e)
    {
        var help = new AsciiHelpWindow();
        help.ShowDialog();
    }
}
