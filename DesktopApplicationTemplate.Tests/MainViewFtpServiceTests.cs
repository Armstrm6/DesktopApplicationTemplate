using System;
using System.IO;
using System.Threading;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class MainViewFtpServiceTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void AddFtpService_SetsOptionsAndAddsService()
    {
        var thread = new Thread(() =>
        {
            var logger = new Mock<ILoggingService>();
            var fileDialog = new Mock<IFileDialogService>();
            var csvVm = new CsvViewerViewModel(fileDialog.Object);
            var csvService = new CsvService(csvVm);
            var netSvc = new Mock<INetworkConfigurationService>();
            netSvc.Setup(s => s.GetConfigurationAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new NetworkConfiguration());
            var netVm = new NetworkConfigurationViewModel(netSvc.Object);
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, string.Empty);
            var mainVm = new MainViewModel(csvService, netVm, netSvc.Object, logger.Object, tempFile);

            var ftpSvc = new Mock<IFtpServerService>();
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(s =>
                {
                    s.AddLogging();
                    s.AddSingleton<ILoggingService>(logger.Object);
                    s.AddSingleton<IFtpServerService>(ftpSvc.Object);
                    s.AddSingleton<FtpServiceViewModel>();
                    s.AddSingleton<FTPServiceView>();
                    s.AddOptions<FtpServerOptions>();
                })
                .Build();
            var prop = typeof(App).GetProperty("AppHost");
            var setter = prop!.GetSetMethod(true);
            setter!.Invoke(null, new object[] { host });

            var view = new MainView(mainVm);
            var opts = new FtpServerOptions { Port = 2121, RootPath = "/tmp", AllowAnonymous = true };

            view.AddFtpService("Test", opts);

            mainVm.Services.Should().HaveCount(1);
            var added = mainVm.Services[0];
            added.FtpOptions.Should().BeSameAs(opts);
            var bound = host.Services.GetRequiredService<IOptions<FtpServerOptions>>().Value;
            bound.Port.Should().Be(2121);
            bound.RootPath.Should().Be("/tmp");
            ConsoleTestLogger.LogPass();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }
}
