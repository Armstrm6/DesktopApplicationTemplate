using System;
using System.IO;
using System.Threading;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;
using DesktopApplicationTemplate.UI.Helpers;
using System.Windows.Controls;
using System.Windows.Input;

namespace DesktopApplicationTemplate.Tests;

public class MainViewHeartbeatNavigationTests
{
    [WpfFact]
    public void NavigateToHeartbeat_ShowsCreateView()
    {
        Skip.IfNot(OperatingSystem.IsWindows(), "Requires Windows desktop runtime");

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
                    s.AddTransient<HeartbeatCreateServiceViewModel>();
                    s.AddTransient<HeartbeatCreateServiceView>();
                    s.AddTransient<HeartbeatAdvancedConfigViewModel>();
                    s.AddTransient<HeartbeatAdvancedConfigView>();
                })
                .Build();
            var prop = typeof(App).GetProperty("AppHost");
            var setter = prop!.GetSetMethod(true);
            setter!.Invoke(null, new object[] { host });

            var view = new MainView(mainVm);
            var method = typeof(MainView).GetMethod("NavigateToHeartbeat", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method!.Invoke(view, new object[] { "Test" });

            view.ContentFrame.Content.Should().BeOfType<HeartbeatCreateServiceView>();
            ConsoleTestLogger.LogPass();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }

    [WpfFact]
    public void EditHeartbeatService_ShowsEditView()
    {
        Skip.IfNot(OperatingSystem.IsWindows(), "Requires Windows desktop runtime");

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
                    s.AddSingleton<HeartbeatViewModel>();
                    s.AddSingleton<HeartbeatView>();
                    s.AddTransient<HeartbeatEditServiceViewModel>();
                    s.AddTransient<HeartbeatEditServiceView>();
                    s.AddTransient<HeartbeatAdvancedConfigViewModel>();
                    s.AddTransient<HeartbeatAdvancedConfigView>();
                })
                .Build();
            var prop = typeof(App).GetProperty("AppHost");
            var setter = prop!.GetSetMethod(true);
            setter!.Invoke(null, new object[] { host });

            var service = new ServiceViewModel
            {
                DisplayName = "Heartbeat - Test",
                ServiceType = "Heartbeat",
                HeartbeatOptions = new HeartbeatServiceOptions { BaseMessage = "msg" }
            };

            var view = new MainView(mainVm);
            var method = typeof(MainView).GetMethod("OnEditRequested", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method!.Invoke(view, new object[] { service });

            view.ContentFrame.Content.Should().BeOfType<HeartbeatEditServiceView>();
            ConsoleTestLogger.LogPass();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }

    [WpfFact]
    public void DoubleClickHeartbeatService_OpensEditView()
    {
        Skip.IfNot(OperatingSystem.IsWindows(), "Requires Windows desktop runtime");

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
                    s.AddSingleton<HeartbeatViewModel>();
                    s.AddSingleton<HeartbeatView>();
                    s.AddTransient<HeartbeatEditServiceViewModel>();
                    s.AddTransient<HeartbeatEditServiceView>();
                    s.AddTransient<HeartbeatAdvancedConfigViewModel>();
                    s.AddTransient<HeartbeatAdvancedConfigView>();
                })
                .Build();
            var prop = typeof(App).GetProperty("AppHost");
            var setter = prop!.GetSetMethod(true);
            setter!.Invoke(null, new object[] { host });

            var service = new ServiceViewModel
            {
                DisplayName = "Heartbeat - Test",
                ServiceType = "Heartbeat",
                HeartbeatOptions = new HeartbeatServiceOptions { BaseMessage = "msg" }
            };

            var view = new MainView(mainVm);
            var border = new Border { DataContext = service };
            var args = MouseButtonEventArgsFactory.Create(MouseButton.Left, 2);
            args.RoutedEvent = Border.MouseLeftButtonDownEvent;
            var method = typeof(MainView).GetMethod("ServiceItem_MouseLeftButtonDown", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method!.Invoke(view, new object[] { border, args });

            view.ContentFrame.Content.Should().BeOfType<HeartbeatEditServiceView>();
            ConsoleTestLogger.LogPass();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }
}
