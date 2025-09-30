using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Services.Common.Descriptors;

public sealed class HeartbeatServiceDescriptor : ServiceDescriptorBase
{
    public const string DescriptorId = ServiceDescriptorIds.Heartbeat;

    public HeartbeatServiceDescriptor(
        IServiceOptionsSerializer? optionsSerializer = null,
        IReadOnlyCollection<ServiceFactoryBinding>? factories = null)
        : base(
            DescriptorId,
            "Heartbeat",
            "Monitoring",
            "Emit periodic heartbeat messages for monitoring integrations.",
            ServiceType.Heartbeat,
            optionsSerializer,
            factories)
    {
    }
}
