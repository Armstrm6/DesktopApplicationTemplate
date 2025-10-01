using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Mqtt.Descriptors;
using DesktopApplicationTemplate.Services.Mqtt.UI.ViewModels.Mqtt.Advanced;
using DesktopApplicationTemplate.UI.Configuration;

namespace DesktopApplicationTemplate.Services.Mqtt.UI.Views.Mqtt.Advanced;

[ServiceDescriptorRegistration(MqttServiceDescriptor.DescriptorId, ServiceRegistrationKind.AdvancedView)]
public partial class MqttAdvancedConfigView : Page
{
    private readonly ILoggingService _logger;

    public MqttAdvancedConfigView(ILoggingService logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    public void Initialize(MqttAdvancedConfigViewModel vm)
    {
        DataContext = vm;
        vm.Logger = _logger;
    }
}
