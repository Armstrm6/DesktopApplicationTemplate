using DesktopApplicationTemplate.Core.Services.Protocols.Http;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;

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
            ServiceType.Http,
            new ServicePresentationMetadata(
                "🌐",
                "LightGreen",
                "DarkGreen",
                "HTTP"))
    {
    }
}
