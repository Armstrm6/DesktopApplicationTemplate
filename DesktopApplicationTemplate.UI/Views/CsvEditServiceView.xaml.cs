using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views;

public partial class CsvEditServiceView : Page
{
    private readonly ILoggingService _logger;

    public CsvEditServiceView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(CsvEditServiceViewModel vm)
    {
        DataContext = vm;
        vm.Logger = _logger;
    }
}
