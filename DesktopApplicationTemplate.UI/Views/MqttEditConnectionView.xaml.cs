using System.Windows.Controls;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Views;

public partial class MqttEditConnectionView : Page
{
    public MqttEditConnectionView(MqttEditConnectionViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
