using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Hid;
using DesktopApplicationTemplate.Models;
using ModelsServiceType = DesktopApplicationTemplate.Models.ServiceType;

namespace DesktopApplicationTemplate.Core.Modules.BuiltIn;

/// <summary>
/// Describes the built-in HID service integration.
/// </summary>
public sealed class HidServiceDescriptor : BuiltInServiceDescriptor<HidServiceOptions>
{
    public HidServiceDescriptor()
        : base(
            ServiceDescriptorIds.Hid,
            "HID",
            BuiltInServiceCategories.Automation,
            ModelsServiceType.Hid,
            new ServicePresentationMetadata(
                "🎛️",
                "LightYellow",
                "Goldenrod",
                "HID"))
    {
    }
}
