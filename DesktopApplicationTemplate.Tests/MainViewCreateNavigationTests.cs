using System;
using System.IO;
using System.Threading;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Service.Services;
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

namespace DesktopApplicationTemplate.Tests;

public class MainViewCreateNavigationTests
{
    [WpfFact]
    public void NavigateToTcp_ShowsCreateView()
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
                    s.AddTransient<TcpCreateServiceViewModel>();
                    s.AddTransient<TcpCreateServiceView>();
                    s.AddOptions<TcpServiceOptions>();
                })
                .Build();
            var prop = typeof(App).GetProperty("AppHost");
            var setter = prop!.GetSetMethod(true);
            setter!.Invoke(null, new object[] { host });

            var view = new MainView(mainVm);
            var method = typeof(MainView).GetMethod("NavigateToTcp", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method!.Invoke(view, new object[] { "Test" });

            view.ContentFrame.Content.Should().BeOfType<TcpCreateServiceView>();
            ConsoleTestLogger.LogPass();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }

    [WpfFact]
    public void NavigateToFtpServer_ShowsCreateView()
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
                    s.AddSingleton<IServiceRule, ServiceRule>();
                    s.AddTransient(typeof(IServiceScreen<FtpServerOptions>), typeof(ServiceScreen<FtpServerOptions>));
                    s.AddTransient<FtpServerCreateViewModel>();
                    s.AddTransient<FtpServerCreateView>();
                    s.AddOptions<FtpServerOptions>();
                })
                .Build();
            var prop = typeof(App).GetProperty("AppHost");
            var setter = prop!.GetSetMethod(true);
            setter!.Invoke(null, new object[] { host });

            var view = new MainView(mainVm);
            var method = typeof(MainView).GetMethod("NavigateToFtpServer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method!.Invoke(view, new object[] { "Test" });

            view.ContentFrame.Content.Should().BeOfType<FtpServerCreateView>();
            ConsoleTestLogger.LogPass();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }

    [WpfFact]
    public void NavigateToHid_ShowsCreateView()
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
                    s.AddSingleton<SaveConfirmationHelper>();
                    s.AddSingleton<HidViewModel>();
                    s.AddSingleton<HidViews>();
                    s.AddTransient<HidCreateServiceViewModel>();
                    s.AddTransient<HidCreateServiceView>();
                    s.AddTransient<HidAdvancedConfigViewModel>();
                    s.AddTransient<HidAdvancedConfigView>();
                })
                .Build();
            var prop = typeof(App).GetProperty("AppHost");
            var setter = prop!.GetSetMethod(true);
            setter!.Invoke(null, new object[] { host });

            var view = new MainView(mainVm);
            var method = typeof(MainView).GetMethod("NavigateToHid", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method!.Invoke(view, new object[] { "Test" });

            view.ContentFrame.Content.Should().BeOfType<HidCreateServiceView>();
            ConsoleTestLogger.LogPass();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }

    [WpfFact]
    public void NavigateToFileObserver_ShowsCreateView()
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
                    s.AddSingleton<SaveConfirmationHelper>();
                    s.AddSingleton<IFileSearchService>(new Mock<IFileSearchService>().Object);
                    s.AddSingleton<FileObserverViewModel>();
                    s.AddSingleton<FileObserverView>();
                    s.AddTransient<FileObserverCreateServiceViewModel>();
                    s.AddTransient<FileObserverCreateServiceView>();
                    s.AddTransient<FileObserverAdvancedConfigViewModel>();
                    s.AddTransient<FileObserverAdvancedConfigView>();
                })
                .Build();
            var prop = typeof(App).GetProperty("AppHost");
            var setter = prop!.GetSetMethod(true);
            setter!.Invoke(null, new object[] { host });

            var view = new MainView(mainVm);
            var method = typeof(MainView).GetMethod("NavigateToFileObserver", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method!.Invoke(view, new object[] { "Test" });

            view.ContentFrame.Content.Should().BeOfType<FileObserverCreateServiceView>();
            ConsoleTestLogger.LogPass();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }

    [WpfFact]
    public void NavigateToScp_ShowsCreateView()
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
                    s.AddTransient<ScpCreateServiceViewModel>();
                    s.AddTransient<ScpCreateServiceView>();
                    s.AddTransient<ScpAdvancedConfigViewModel>();
                    s.AddTransient<ScpAdvancedConfigView>();
                    s.AddOptions<ScpServiceOptions>();
                })
                .Build();
            var prop = typeof(App).GetProperty("AppHost");
            var setter = prop!.GetSetMethod(true);
            setter!.Invoke(null, new object[] { host });

            var view = new MainView(mainVm);
            var method = typeof(MainView).GetMethod("NavigateToScp", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method!.Invoke(view, new object[] { "Test" });

            view.ContentFrame.Content.Should().BeOfType<ScpCreateServiceView>();
            ConsoleTestLogger.LogPass();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }
}
