using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Mqtt.Descriptors;
using DesktopApplicationTemplate.Services.Mqtt.UI.ViewModels.Mqtt.Edit;
using DesktopApplicationTemplate.UI.Configuration;

namespace DesktopApplicationTemplate.Services.Mqtt.UI.Views.Mqtt.Edit;

[ServiceDescriptorRegistration(MqttServiceDescriptor.DescriptorId, ServiceRegistrationKind.EditView)]
public partial class MqttEditServiceView : Page
{
    private readonly ILoggingService _logger;

    public MqttEditServiceView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(MqttEditServiceViewModel vm)
    {
        DataContext = vm;
        vm.Logger = _logger;
    }
}
