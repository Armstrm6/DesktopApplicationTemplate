using DesktopApplicationTemplate.Core.Services.Protocols.Ftp;
using DesktopApplicationTemplate.Models;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Core.Modules.BuiltIn;

/// <summary>
/// Registers the built-in FTP server services and descriptor.
/// </summary>
public sealed class FtpServiceModule : BuiltInServiceModule<FtpServiceDescriptor>
{
    public override ServiceType Type => ServiceType.Ftp;

    public override void RegisterServices(IServiceCollection services)
    {
        base.RegisterServices(services);
        services.AddFtpServer(builder => builder
            .UseDotNetFileSystem()
            .EnableAnonymousAuthentication());
        services.AddSingleton<IFtpServerService, FtpServerService>();
    }
}
