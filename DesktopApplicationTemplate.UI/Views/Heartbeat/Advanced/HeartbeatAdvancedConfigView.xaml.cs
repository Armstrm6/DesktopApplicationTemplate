using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Heartbeat.Advanced;

namespace DesktopApplicationTemplate.UI.Views.Heartbeat.Advanced;

public partial class HeartbeatAdvancedConfigView : Page
{
    private readonly ILoggingService _logger;

    public HeartbeatAdvancedConfigView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(HeartbeatAdvancedConfigViewModel vm)
    {
        DataContext = vm;
        vm.Logger = _logger;
    }
}
