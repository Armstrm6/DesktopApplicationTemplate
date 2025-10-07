using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Scp;
using DesktopApplicationTemplate.Models;

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
            ServiceType.Scp,
            new ServicePresentationMetadata(
                "📦",
                "LightCyan",
                "CadetBlue",
                "SCP"))
    {
    }
}
