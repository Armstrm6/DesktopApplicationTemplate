using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Core.Modules.BuiltIn;

/// <summary>
/// Registers the built-in Heartbeat descriptor.
/// </summary>
public sealed class HeartbeatServiceModule : BuiltInServiceModule<HeartbeatServiceDescriptor>
{
    public override ServiceType Type => ServiceType.Heartbeat;
}
