using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views;

public partial class MqttEditServiceView : Page
{
    public MqttEditServiceView(MqttEditServiceViewModel vm, ILoggingService logger)
    {
        InitializeComponent();
        DataContext = vm;
        vm.Logger = logger;
    }
}
