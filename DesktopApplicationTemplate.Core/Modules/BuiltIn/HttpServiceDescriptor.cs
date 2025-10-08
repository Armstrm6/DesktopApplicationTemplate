using DesktopApplicationTemplate.Core.Services.Protocols.Http;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using ModelsServiceType = DesktopApplicationTemplate.Models.ServiceType;

namespace DesktopApplicationTemplate.Core.Modules.BuiltIn;

/// <summary>
/// Describes the built-in HTTP client service.
/// </summary>
public sealed class HttpServiceDescriptor : BuiltInServiceDescriptor<HttpServiceOptions>
{
    public HttpServiceDescriptor()
        : base(
            ServiceDescriptorIds.Http,
            "HTTP",
            BuiltInServiceCategories.Connectivity,
            ModelsServiceType.Http,
            new ServicePresentationMetadata(
                "🌐",
                "LightGreen",
                "DarkGreen",
                "HTTP"))
    {
    }
}
