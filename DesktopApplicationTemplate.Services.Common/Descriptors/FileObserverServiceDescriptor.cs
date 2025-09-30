using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Services.Common.Descriptors;

public sealed class FileObserverServiceDescriptor : ServiceDescriptorBase
{
    public const string DescriptorId = ServiceDescriptorIds.FileObserver;

    public FileObserverServiceDescriptor(
        IServiceOptionsSerializer? optionsSerializer = null,
        IReadOnlyCollection<ServiceFactoryBinding>? factories = null)
        : base(
            DescriptorId,
            "File Observer",
            "File",
            "Monitor directories for changes and trigger automation flows.",
            ServiceType.FileObserver,
            optionsSerializer,
            factories,
            new ServicePresentationMetadata(
                null,
                "#FFFFA07A",
                "#FFE9967A",
                "File Observer"))
    {
    }
}
