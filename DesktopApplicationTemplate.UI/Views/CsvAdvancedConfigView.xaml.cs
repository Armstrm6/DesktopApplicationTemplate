using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views;

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
