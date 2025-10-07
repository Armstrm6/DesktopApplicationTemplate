using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.FileObserver;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Core.Modules.BuiltIn;

/// <summary>
/// Describes the built-in File Observer service.
/// </summary>
public sealed class FileObserverServiceDescriptor : BuiltInServiceDescriptor<FileObserverServiceOptions>
{
    public FileObserverServiceDescriptor()
        : base(
            ServiceDescriptorIds.FileObserver,
            "File Observer",
            BuiltInServiceCategories.Monitoring,
            ServiceType.FileObserver,
            new ServicePresentationMetadata(
                "👀",
                "LightSalmon",
                "DarkSalmon",
                "File Observer"))
    {
    }
}
