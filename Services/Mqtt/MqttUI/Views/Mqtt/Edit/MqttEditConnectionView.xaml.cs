using System.Windows.Controls;
using DesktopApplicationTemplate.Services.Mqtt.Descriptors;
using DesktopApplicationTemplate.Services.Mqtt.UI.ViewModels.Mqtt.Edit;
using DesktopApplicationTemplate.UI.Configuration;

namespace DesktopApplicationTemplate.Services.Mqtt.UI.Views.Mqtt.Edit;

[ServiceDescriptorRegistration(MqttServiceDescriptor.DescriptorId, ServiceRegistrationKind.SupplementalView)]
public partial class MqttEditConnectionView : Page
{
    public MqttEditConnectionView(MqttEditConnectionViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
