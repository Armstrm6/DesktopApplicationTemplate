using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views;

public partial class HeartbeatEditServiceView : Page
{
    private readonly ILoggingService _logger;

    public HeartbeatEditServiceView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(HeartbeatEditServiceViewModel vm)
    {
        DataContext = vm;
        vm.Logger = _logger;
    }
}
