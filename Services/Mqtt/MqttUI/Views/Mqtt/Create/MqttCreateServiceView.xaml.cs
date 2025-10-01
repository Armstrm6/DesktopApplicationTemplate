using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Mqtt.Descriptors;
using DesktopApplicationTemplate.Services.Mqtt.UI.ViewModels.Mqtt.Create;
using DesktopApplicationTemplate.UI.Configuration;

namespace DesktopApplicationTemplate.Services.Mqtt.UI.Views.Mqtt.Create;

[ServiceDescriptorRegistration(MqttServiceDescriptor.DescriptorId, ServiceRegistrationKind.CreateView)]
public partial class MqttCreateServiceView : Page
{
    public MqttCreateServiceView(MqttCreateServiceViewModel vm, ILoggingService logger)
    {
        InitializeComponent();
        DataContext = vm;
        vm.Logger = logger;
    }
}
