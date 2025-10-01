using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services.Common.Descriptors;
using Microsoft.Extensions.DependencyInjection;
using TcpRelayPlugin.Runtime;

namespace TcpRelayPlugin.Descriptors;

public sealed class TcpRelayServiceDescriptor : ServiceDescriptorBase
{
    public const string DescriptorId = "sample.services.tcp";

    public TcpRelayServiceDescriptor(IServiceOptionsSerializer? optionsSerializer = null)
        : base(
            DescriptorId,
            "Sample TCP Relay",
            "Networking",
            "Demonstrates packaging the TCP descriptor as a plug-in.",
            ServiceType.Tcp,
            optionsSerializer,
            CreateFactories(),
            new ServicePresentationMetadata(
                "🔁",
                "#FF0EA5E9",
                "#FF1E3A8A",
                "TCP Relay"))
    {
    }

    private static IReadOnlyCollection<ServiceFactoryBinding> CreateFactories() =>
    [
        ServiceFactoryBinding.Create(
            ServiceFactoryKind.Runtime,
            typeof(IServiceRuntimeFactory),
            services => services.GetRequiredService<TcpRelayRuntimeFactory>())
    ];
}
