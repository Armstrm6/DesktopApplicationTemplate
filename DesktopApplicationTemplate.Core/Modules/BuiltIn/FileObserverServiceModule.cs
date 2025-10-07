using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Core.Modules.BuiltIn;

/// <summary>
/// Registers the built-in File Observer descriptor.
/// </summary>
public sealed class FileObserverServiceModule : BuiltInServiceModule<FileObserverServiceDescriptor>
{
    public override ServiceType Type => ServiceType.FileObserver;
}
