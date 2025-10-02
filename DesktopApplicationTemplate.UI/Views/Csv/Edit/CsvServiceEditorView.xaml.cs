using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Csv.Edit;
using System.Windows.Controls;

namespace DesktopApplicationTemplate.UI.Views.Csv.Edit;

public partial class CsvServiceEditorView : Page
{
    private readonly ILoggingService _logger;

    public CsvServiceEditorView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(CsvServiceEditorViewModel vm)
    {
        DataContext = vm;
        vm.Logger = _logger;
    }
}
