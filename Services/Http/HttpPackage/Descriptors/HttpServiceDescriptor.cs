using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services.Common.Descriptors;

namespace DesktopApplicationTemplate.Services.Http.Descriptors;

public sealed class HttpServiceDescriptor : ServiceDescriptorBase
{
    public const string DescriptorId = ServiceDescriptorIds.Http;

    public HttpServiceDescriptor(
        IServiceOptionsSerializer? optionsSerializer = null,
        IReadOnlyCollection<ServiceFactoryBinding>? factories = null)
        : base(
            DescriptorId,
            "HTTP",
            "Networking",
            "Interact with HTTP endpoints for automation workflows.",
            ServiceType.Http,
            optionsSerializer,
            factories,
            new ServicePresentationMetadata(
                "🌐",
                "#FF90EE90",
                "#FF006400",
                "HTTP"))
    {
    }
}
