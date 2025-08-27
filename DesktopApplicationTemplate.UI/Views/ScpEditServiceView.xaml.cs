using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views;

public partial class ScpEditServiceView : Page
{
    private readonly ILoggingService _logger;

    public ScpEditServiceView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(ScpEditServiceViewModel vm)
    {
        DataContext = vm;
        vm.Logger = _logger;
    }
}
