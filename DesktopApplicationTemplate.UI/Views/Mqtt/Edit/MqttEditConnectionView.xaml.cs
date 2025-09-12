using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt.Edit;

namespace DesktopApplicationTemplate.UI.Views.Mqtt.Edit;

public partial class MqttEditConnectionView : Page
{
    public MqttEditConnectionView(MqttEditConnectionViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
