using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Services.Hid.Descriptors;

public sealed class HidServiceDescriptor : ServiceDescriptorBase
{
    public const string DescriptorId = ServiceDescriptorIds.Hid;

    public HidServiceDescriptor(
        IServiceOptionsSerializer? optionsSerializer = null,
        IReadOnlyCollection<ServiceFactoryBinding>? factories = null)
        : base(
            DescriptorId,
            "HID",
            "Hardware",
            "Interact with Human Interface Devices for automation scenarios.",
            ServiceType.Hid,
            optionsSerializer,
            factories,
            new ServicePresentationMetadata(
                null,
                "#FFFFFFE0",
                "#FFDAA520",
                "HID"))
    {
    }
}
