using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI;
using DesktopApplicationTemplate.UI.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;
using System.Reflection;

namespace DesktopApplicationTemplate.Tests;

public class MainViewTcpEditTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void EditTcpService_SavesUpdatedOptions()
    {
        var thread = new Thread(() =>
        {
            var logger = new LoggingService(new NullRichTextLogger());
            var startup = new Mock<IStartupService>();
            startup.Setup(s => s.RunStartupChecksAsync()).Returns(Task.CompletedTask);
            startup.Setup(s => s.GetSettings()).Returns(new AppSettings());

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(s =>
                {
                    s.AddSingleton<IRichTextLogger, NullRichTextLogger>();
                    s.AddSingleton<ILoggingService>(logger);
                    s.AddSingleton<IStartupService>(startup.Object);
                    s.AddSingleton<SaveConfirmationHelper>();
                    s.AddSingleton<TcpServiceViewModel>();
                    s.AddSingleton<TcpServiceView>();
                    s.AddSingleton<TcpServiceMessagesViewModel>();
                    s.AddSingleton<TcpServiceMessagesView>();
                })
                .Build();
            var prop = typeof(App).GetProperty("AppHost");
            var setter = prop!.GetSetMethod(true);
            setter!.Invoke(null, new object[] { host });

            var fileDialog = new Mock<IFileDialogService>();
            var csvVm = new CsvViewerViewModel(fileDialog.Object);
            var csvService = new CsvService(csvVm);
            var netSvc = new Mock<INetworkConfigurationService>();
            netSvc.Setup(s => s.GetConfigurationAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new NetworkConfiguration());
            var netVm = new NetworkConfigurationViewModel(netSvc.Object, logger);
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, string.Empty);
            var mainVm = new MainViewModel(csvService, netVm, netSvc.Object, logger, tempFile);

            var service = new ServiceViewModel
            {
                DisplayName = "TCP - Test",
                ServiceType = "TCP",
                TcpOptions = new TcpServiceOptions { Host = "1.1.1.1", Port = 1234, UseUdp = false, Mode = TcpServiceMode.Listening }
            };

            var view = new MainView(mainVm);
            var editMethod = typeof(MainView).GetMethod("OnEditRequested", BindingFlags.Instance | BindingFlags.NonPublic);
            editMethod!.Invoke(view, new object[] { service });

            var tcpView = host.Services.GetRequiredService<TcpServiceView>();
            var vm = (TcpServiceViewModel)tcpView.DataContext;
            vm.ComputerIp = "127.0.0.1";
            vm.ListeningPort = "9000";
            vm.IsUdp = true;
            vm.SelectedMode = "Sending";
            var eventField = typeof(TcpServiceViewModel).GetField("RequestClose", BindingFlags.Instance | BindingFlags.NonPublic);
            var handler = (EventHandler?)eventField?.GetValue(vm);
            handler?.Invoke(vm, EventArgs.Empty);

            service.TcpOptions.Should().NotBeNull();
            service.TcpOptions!.Host.Should().Be("127.0.0.1");
            service.TcpOptions.Port.Should().Be(9000);
            service.TcpOptions.UseUdp.Should().BeTrue();
            service.TcpOptions.Mode.Should().Be(TcpServiceMode.Sending);
            File.ReadAllText(tempFile).Should().Contain("127.0.0.1");
            ConsoleTestLogger.LogPass();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }
}
