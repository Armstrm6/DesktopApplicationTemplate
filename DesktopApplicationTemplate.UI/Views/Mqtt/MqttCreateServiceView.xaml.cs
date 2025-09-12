using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt.Create;

namespace DesktopApplicationTemplate.UI.Views.Mqtt;

public partial class MqttCreateServiceView : Page
{
    public MqttCreateServiceView(MqttCreateServiceViewModel vm, ILoggingService logger)
    {
        InitializeComponent();
        DataContext = vm;
        vm.Logger = logger;
    }
}
