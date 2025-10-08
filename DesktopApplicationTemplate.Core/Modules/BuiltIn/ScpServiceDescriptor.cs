using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Scp;
using DesktopApplicationTemplate.Models;
using ModelsServiceType = DesktopApplicationTemplate.Models.ServiceType;

namespace DesktopApplicationTemplate.Core.Modules.BuiltIn;

/// <summary>
/// Describes the built-in SCP transfer service.
/// </summary>
public sealed class ScpServiceDescriptor : BuiltInServiceDescriptor<ScpServiceOptions>
{
    public ScpServiceDescriptor()
        : base(
            ServiceDescriptorIds.Scp,
            "SCP",
            BuiltInServiceCategories.FileTransfer,
            ModelsServiceType.Scp,
            new ServicePresentationMetadata(
                "📦",
                "LightCyan",
                "CadetBlue",
                "SCP"))
    {
    }
}
