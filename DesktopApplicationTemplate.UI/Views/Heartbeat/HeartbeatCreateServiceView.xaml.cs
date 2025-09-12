using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Heartbeat;

namespace DesktopApplicationTemplate.UI.Views.Heartbeat;

public partial class HeartbeatCreateServiceView : Page
{
    public HeartbeatCreateServiceView(HeartbeatCreateServiceViewModel vm, ILoggingService logger)
    {
        InitializeComponent();
        DataContext = vm;
        vm.Logger = logger;
    }
}
