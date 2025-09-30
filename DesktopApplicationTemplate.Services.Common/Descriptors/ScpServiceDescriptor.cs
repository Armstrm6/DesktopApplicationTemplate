using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Services.Common.Descriptors;

public sealed class ScpServiceDescriptor : ServiceDescriptorBase
{
    public const string DescriptorId = ServiceDescriptorIds.Scp;

    public ScpServiceDescriptor(
        IServiceOptionsSerializer? optionsSerializer = null,
        IReadOnlyCollection<ServiceFactoryBinding>? factories = null)
        : base(
            DescriptorId,
            "SCP",
            "Networking",
            "Transfer files securely using the SCP protocol.",
            ServiceType.Scp,
            optionsSerializer,
            factories)
    {
    }
}
