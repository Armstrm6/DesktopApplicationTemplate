using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Heartbeat;
using DesktopApplicationTemplate.Models;
using ModelsServiceType = DesktopApplicationTemplate.Models.ServiceType;

namespace DesktopApplicationTemplate.Core.Modules.BuiltIn;

/// <summary>
/// Describes the built-in Heartbeat service.
/// </summary>
public sealed class HeartbeatServiceDescriptor : BuiltInServiceDescriptor<HeartbeatServiceOptions>
{
    public HeartbeatServiceDescriptor()
        : base(
            ServiceDescriptorIds.Heartbeat,
            "Heartbeat",
            BuiltInServiceCategories.Monitoring,
            ModelsServiceType.Heartbeat,
            new ServicePresentationMetadata(
                "❤️",
                "LightPink",
                "DeepPink",
                "Heartbeat"))
    {
    }
}
