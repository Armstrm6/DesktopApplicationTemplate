using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Csv;

namespace DesktopApplicationTemplate.UI.Views.Csv;

public partial class CsvAdvancedConfigView : Page
{
    private readonly ILoggingService _logger;

    public CsvAdvancedConfigView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(CsvAdvancedConfigViewModel vm)
    {
        DataContext = vm;
        vm.Logger = _logger;
    }
}
