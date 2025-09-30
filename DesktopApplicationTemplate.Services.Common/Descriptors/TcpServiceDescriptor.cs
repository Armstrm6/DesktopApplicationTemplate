using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Services.Common.Descriptors;

public sealed class TcpServiceDescriptor : ServiceDescriptorBase
{
    public const string DescriptorId = ServiceDescriptorIds.Tcp;

    public TcpServiceDescriptor(
        IServiceOptionsSerializer? optionsSerializer = null,
        IReadOnlyCollection<ServiceFactoryBinding>? factories = null)
        : base(
            DescriptorId,
            "TCP",
            "Networking",
            "Send and receive messages over TCP or UDP.",
            ServiceType.Tcp,
            optionsSerializer,
            factories)
    {
    }
}
