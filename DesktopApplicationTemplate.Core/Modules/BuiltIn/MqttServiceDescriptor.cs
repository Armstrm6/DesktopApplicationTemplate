using DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using ModelsServiceType = DesktopApplicationTemplate.Models.ServiceType;

namespace DesktopApplicationTemplate.Core.Modules.BuiltIn;

/// <summary>
/// Describes the built-in MQTT client service.
/// </summary>
public sealed class MqttServiceDescriptor : BuiltInServiceDescriptor<MqttServiceOptions>
{
    public MqttServiceDescriptor()
        : base(
            ServiceDescriptorIds.Mqtt,
            "MQTT",
            BuiltInServiceCategories.Messaging,
            ModelsServiceType.Mqtt,
            new ServicePresentationMetadata(
                "📡",
                "LightGoldenrodYellow",
                "Goldenrod",
                "MQTT"))
    {
    }
}
