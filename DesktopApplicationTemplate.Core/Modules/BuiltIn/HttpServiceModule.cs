using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Core.Modules.BuiltIn;

/// <summary>
/// Registers the built-in HTTP service descriptor.
/// </summary>
public sealed class HttpServiceModule : BuiltInServiceModule<HttpServiceDescriptor>
{
    public override ServiceType Type => ServiceType.Http;
}
