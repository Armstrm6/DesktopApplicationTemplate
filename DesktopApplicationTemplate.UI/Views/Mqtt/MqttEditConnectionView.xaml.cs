using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt;

namespace DesktopApplicationTemplate.UI.Views.Mqtt;

public partial class MqttEditConnectionView : Page
{
    public MqttEditConnectionView(MqttEditConnectionViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
