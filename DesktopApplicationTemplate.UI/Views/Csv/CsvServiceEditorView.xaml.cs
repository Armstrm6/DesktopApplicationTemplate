using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Csv;
using System.Windows.Controls;

namespace DesktopApplicationTemplate.UI.Views.Csv;

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
