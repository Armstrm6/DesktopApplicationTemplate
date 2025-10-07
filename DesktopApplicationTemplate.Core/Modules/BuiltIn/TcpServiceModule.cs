using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Core.Modules.BuiltIn;

/// <summary>
/// Registers the built-in TCP descriptor.
/// </summary>
public sealed class TcpServiceModule : BuiltInServiceModule<TcpServiceDescriptor>
{
    public override ServiceType Type => ServiceType.Tcp;
}
