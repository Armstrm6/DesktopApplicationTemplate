using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Tcp;
using DesktopApplicationTemplate.Models;
using ModelsServiceType = DesktopApplicationTemplate.Models.ServiceType;

namespace DesktopApplicationTemplate.Core.Modules.BuiltIn;

/// <summary>
/// Describes the built-in TCP scripting service.
/// </summary>
public sealed class TcpServiceDescriptor : BuiltInServiceDescriptor<TcpServiceOptions>
{
    public TcpServiceDescriptor()
        : base(
            ServiceDescriptorIds.Tcp,
            "TCP",
            BuiltInServiceCategories.Connectivity,
            ModelsServiceType.Tcp,
            new ServicePresentationMetadata(
                "🔗",
                "LightBlue",
                "DarkBlue",
                "TCP"))
    {
    }
}
