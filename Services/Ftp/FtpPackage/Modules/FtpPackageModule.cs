using System;
using System.Collections.Generic;
using System.IO;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Ftp.Descriptors;
using DesktopApplicationTemplate.Services.Ftp.Options;
using DesktopApplicationTemplate.Services.Ftp.Security;
using DesktopApplicationTemplate.Services.Ftp.Services;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer;
using FubarDev.FtpServer.DependencyInjection;
using FubarDev.FtpServer.FileSystem.DotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DesktopApplicationTemplate.Services.Ftp.Modules;

/// <summary>
/// Registers the FTP server package services.
/// </summary>
public sealed class FtpPackageModule : IServiceModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services
            .AddOptions<FtpServerHostOptions>()
            .BindConfiguration("FtpServer")
            .ValidateDataAnnotations();

        services.AddSingleton<FtpServiceDescriptor>();
        services.AddSingleton<IServiceDescriptor>(sp => sp.GetRequiredService<FtpServiceDescriptor>());

        services.AddSingleton<FtpService>();
        services.AddSingleton<IFtpService>(sp => sp.GetRequiredService<FtpService>());

        services.AddSingleton<FtpServerService>();
        services.AddSingleton<IFtpServerService>(sp => sp.GetRequiredService<FtpServerService>());

        services.AddSingleton<ConfiguredMembershipProvider>();
        services.AddSingleton<IMembershipProviderAsync>(sp => sp.GetRequiredService<ConfiguredMembershipProvider>());
        services.AddSingleton<IMembershipProvider>(sp => sp.GetRequiredService<ConfiguredMembershipProvider>());

        services.AddFtpServer(builder => builder.UseDotNetFileSystem());

        services.AddOptions<FtpServerOptions>()
            .Configure<IOptionsMonitor<FtpServerHostOptions>>((options, monitor) =>
            {
                var hostOptions = monitor.CurrentValue;
                options.Port = hostOptions.Port;
                if (!string.IsNullOrWhiteSpace(hostOptions.Address))
                {
                    options.ServerAddress = hostOptions.Address;
                }
            });

        services.AddOptions<DotNetFileSystemOptions>()
            .Configure<IOptionsMonitor<FtpServerHostOptions>>((options, monitor) =>
            {
                var hostOptions = monitor.CurrentValue;
                var root = string.IsNullOrWhiteSpace(hostOptions.RootPath)
                    ? Path.Combine(AppContext.BaseDirectory, "ftp-root")
                    : hostOptions.RootPath;

                var fullPath = Path.GetFullPath(root ?? Path.Combine(AppContext.BaseDirectory, "ftp-root"));
                Directory.CreateDirectory(fullPath);
                options.RootPath = fullPath;
            });
    }

    public IEnumerable<IServiceDescriptor> DescribeServices()
    {
        yield return new FtpServiceDescriptor();
    }
}
