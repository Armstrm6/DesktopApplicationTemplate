using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Services.Common.Descriptors;

public sealed class FtpServiceDescriptor : ServiceDescriptorBase
{
    public const string DescriptorId = ServiceDescriptorIds.Ftp;

    public FtpServiceDescriptor(
        IServiceOptionsSerializer? optionsSerializer = null,
        IReadOnlyCollection<ServiceFactoryBinding>? factories = null)
        : base(
            DescriptorId,
            "FTP",
            "Networking",
            "Transfer files to remote hosts using the FTP protocol.",
            ServiceType.Ftp,
            optionsSerializer,
            factories,
            new ServicePresentationMetadata(
                "🖥️",
                "#FFB0C4DE",
                "#FF4682B4",
                "FTP Server"))
    {
    }
}
