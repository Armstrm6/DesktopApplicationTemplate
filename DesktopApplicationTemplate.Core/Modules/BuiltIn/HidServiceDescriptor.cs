using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Hid;
using DesktopApplicationTemplate.Models;

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
            ServiceType.Hid,
            new ServicePresentationMetadata(
                "🎛️",
                "LightYellow",
                "Goldenrod",
                "HID"))
    {
    }
}
