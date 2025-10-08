using DesktopApplicationTemplate.Core.Services.Protocols.Ftp;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using ModelsServiceType = DesktopApplicationTemplate.Models.ServiceType;

namespace DesktopApplicationTemplate.Core.Modules.BuiltIn;

/// <summary>
/// Describes the built-in FTP server service.
/// </summary>
public sealed class FtpServiceDescriptor : BuiltInServiceDescriptor<FtpServerOptions>
{
    public FtpServiceDescriptor()
        : base(
            ServiceDescriptorIds.Ftp,
            "FTP Server",
            BuiltInServiceCategories.FileTransfer,
            ModelsServiceType.Ftp,
            new ServicePresentationMetadata(
                "🖥️",
                "LightSteelBlue",
                "SteelBlue",
                "FTP"))
    {
    }
}
