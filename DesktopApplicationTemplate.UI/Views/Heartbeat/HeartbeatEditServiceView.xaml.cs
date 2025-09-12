using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Heartbeat.Edit;

namespace DesktopApplicationTemplate.UI.Views.Heartbeat;

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
