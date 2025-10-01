using System;
using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Ftp.UI.EditHandlers;
using DesktopApplicationTemplate.Services.Ftp.UI.Factories;
using DesktopApplicationTemplate.Services.Ftp.UI.Navigation;
using DesktopApplicationTemplate.Services.Ftp.UI.Options;
using DesktopApplicationTemplate.Services.Ftp.UI.ViewModels.Ftp;
using DesktopApplicationTemplate.Services.Ftp.UI.ViewModels.Ftp.Advanced;
using DesktopApplicationTemplate.Services.Ftp.UI.ViewModels.Ftp.Create;
using DesktopApplicationTemplate.Services.Ftp.UI.ViewModels.Ftp.Edit;
using DesktopApplicationTemplate.Services.Ftp.UI.Views.Ftp;
using DesktopApplicationTemplate.Services.Ftp.UI.Views.Ftp.Advanced;
using DesktopApplicationTemplate.Services.Ftp.UI.Views.Ftp.Create;
using DesktopApplicationTemplate.Services.Ftp.UI.Views.Ftp.Edit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Services.Ftp.UI.Modules;

/// <summary>
/// Registers the FTP UI components with the host application.
/// </summary>
public sealed class FtpUiModule : IServiceModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services
            .AddOptions<FtpServerOptions>()
            .BindConfiguration("FtpServer");

        services.AddTransient<FtpServiceFactory>();
        services.AddTransient<FtpNavigationHandler>();
        services.AddTransient<FtpEditServiceHandler>();

        services.AddTransient<FtpServiceViewModel>();
        services.AddTransient<FtpServerCreateViewModel>();
        services.AddTransient<FtpServerEditViewModel>();
        services.AddTransient<FtpServerAdvancedConfigViewModel>();

        services.AddTransient<FtpServerCreateView>();
        services.AddTransient<FtpServerEditView>();
        services.AddTransient<FtpServerAdvancedConfigView>();
        services.AddTransient<FTPServiceView>();
    }

    public IEnumerable<IServiceDescriptor> DescribeServices() => Array.Empty<IServiceDescriptor>();
}
