using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Csv.UI.ViewModels.Csv.Edit;

namespace DesktopApplicationTemplate.Services.Csv.UI.Views.Csv.Edit;

public partial class CsvServiceEditorView : Page
{
    private readonly ILoggingService _logger;

    public CsvServiceEditorView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(CsvServiceEditorViewModel viewModel)
    {
        DataContext = viewModel;
        viewModel.Logger = _logger;
    }
}
