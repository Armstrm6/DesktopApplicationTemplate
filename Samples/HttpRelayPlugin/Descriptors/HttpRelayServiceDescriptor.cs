using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services.Common.Descriptors;
using Microsoft.Extensions.DependencyInjection;
using HttpRelayPlugin.Runtime;

namespace HttpRelayPlugin.Descriptors;

public sealed class HttpRelayServiceDescriptor : ServiceDescriptorBase
{
    public const string DescriptorId = "sample.services.http";

    public HttpRelayServiceDescriptor(IServiceOptionsSerializer? optionsSerializer = null)
        : base(
            DescriptorId,
            "Sample HTTP Relay",
            "Networking",
            "Demonstrates packaging the HTTP descriptor as a plug-in.",
            ServiceType.Http,
            optionsSerializer,
            CreateFactories(),
            new ServicePresentationMetadata(
                "🌐",
                "#FF2563EB",
                "#FF1D4ED8",
                "HTTP Relay"))
    {
    }

    private static IReadOnlyCollection<ServiceFactoryBinding> CreateFactories() =>
    [
        ServiceFactoryBinding.Create(
            ServiceFactoryKind.Runtime,
            typeof(IServiceRuntimeFactory),
            services => services.GetRequiredService<HttpRelayRuntimeFactory>())
    ];
}
