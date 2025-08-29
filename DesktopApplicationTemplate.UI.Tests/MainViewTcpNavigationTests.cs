using System;
using System.IO;
using System.Threading;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class MainViewTcpNavigationTests
{
    [WindowsFact]
    public void CreateTcpService_Advanced_Back_ReturnsToCreateView()
    {

        var thread = new Thread(() =>
        {
            var logger = new LoggingService(new NullRichTextLogger());
            var fileDialog = new Mock<IFileDialogService>();
            var csvVm = new CsvViewerViewModel(fileDialog.Object);
            var csvService = new CsvService(csvVm);
            var netSvc = new Mock<INetworkConfigurationService>();
            netSvc.Setup(s => s.GetConfigurationAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new NetworkConfiguration());
            var netVm = new NetworkConfigurationViewModel(netSvc.Object, logger);
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, string.Empty);
            var mainVm = new MainViewModel(csvService, netVm, netSvc.Object, logger, tempFile);

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(s =>
                {
                    s.AddLogging();
                    s.AddSingleton<ILoggingService>(logger);
                    s.AddTransient<TcpCreateServiceViewModel>();
                    s.AddTransient<TcpCreateServiceView>();
                    s.AddTransient<TcpAdvancedConfigViewModel>();
                    s.AddTransient<TcpAdvancedConfigView>();
                })
                .Build();
            var prop = typeof(App).GetProperty("AppHost");
            var setter = prop!.GetSetMethod(true);
            setter!.Invoke(null, new object[] { host });

            var view = new MainView(mainVm);
            var method = typeof(MainView).GetMethod("NavigateToTcp", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method!.Invoke(view, new object[] { "Test" });

            view.ContentFrame.Content.Should().BeOfType<TcpCreateServiceView>();
            var createView = (TcpCreateServiceView)view.ContentFrame.Content;
            var vm = (TcpCreateServiceViewModel)createView.DataContext;
            vm.AdvancedConfigCommand.Execute(null);

            view.ContentFrame.Content.Should().BeOfType<TcpAdvancedConfigView>();
            var advView = (TcpAdvancedConfigView)view.ContentFrame.Content;
            var advVm = (TcpAdvancedConfigViewModel)advView.DataContext;
            advVm.BackCommand.Execute(null);

            view.ContentFrame.Content.Should().BeOfType<TcpCreateServiceView>();
            ConsoleTestLogger.LogPass();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }
}
