using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Core.Modules.BuiltIn;

/// <summary>
/// Registers the built-in HID descriptor.
/// </summary>
public sealed class HidServiceModule : BuiltInServiceModule<HidServiceDescriptor>
{
    public override ServiceType Type => ServiceType.Hid;
}
