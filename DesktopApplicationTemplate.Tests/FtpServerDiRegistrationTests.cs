using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;

namespace DesktopApplicationTemplate.Tests;

public class FtpServerDiRegistrationTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void ServiceProvider_Resolves_FtpServerTypes()
    {
        var settings = new Dictionary<string, string?>
        {
            ["FtpServer:Port"] = "2121",
            ["FtpServer:RootPath"] = "/tmp",
            ["FtpServer:AllowAnonymous"] = "true"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IRichTextLogger, NullRichTextLogger>();
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<IMessageRoutingService, MessageRoutingService>();
        services.AddOptions<DesktopApplicationTemplate.UI.Services.FtpServerOptions>()
            .BindConfiguration("FtpServer");
        services.AddFtpServer(builder => builder
            .UseDotNetFileSystem()
            .EnableAnonymousAuthentication());
        services.AddSingleton<IFtpServerService, DesktopApplicationTemplate.Service.Services.FtpServerService>();
        services.AddSingleton<FtpServerViewViewModel>();

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IFtpServerService>().Should().NotBeNull();
        provider.GetRequiredService<FtpServerViewViewModel>().Should().NotBeNull();

        var options = provider
            .GetRequiredService<IOptions<DesktopApplicationTemplate.UI.Services.FtpServerOptions>>()
            .Value;
        options.Port.Should().Be(2121);
        options.RootPath.Should().Be("/tmp");
        options.AllowAnonymous.Should().BeTrue();
    }
}
