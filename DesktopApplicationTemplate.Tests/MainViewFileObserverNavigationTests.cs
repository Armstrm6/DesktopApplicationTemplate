using System;
using System.IO;
using System.Threading;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
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

public class MainViewFileObserverNavigationTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void EditFileObserverService_ShowsEditView()
    {
        if (!OperatingSystem.IsWindows())
            return;

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
                    s.AddSingleton<FileObserverViewModel>();
                    s.AddSingleton<FileObserverView>();
                    s.AddTransient<FileObserverEditServiceViewModel>();
                    s.AddTransient<FileObserverEditServiceView>();
                    s.AddTransient<FileObserverAdvancedConfigViewModel>();
                    s.AddTransient<FileObserverAdvancedConfigView>();
                })
                .Build();
            var prop = typeof(App).GetProperty("AppHost");
            var setter = prop!.GetSetMethod(true);
            setter!.Invoke(null, new object[] { host });

            var service = new ServiceViewModel
            {
                DisplayName = "File Observer - Test",
                ServiceType = "File Observer",
                FileObserverOptions = new FileObserverServiceOptions { FilePath = "path" }
            };

            var view = new MainView(mainVm);
            var method = typeof(MainView).GetMethod("OnEditRequested", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method!.Invoke(view, new object[] { service });

            view.ContentFrame.Content.Should().BeOfType<FileObserverEditServiceView>();
            ConsoleTestLogger.LogPass();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
       thread.Join();
    }

    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void DoubleClickFileObserverService_OpensEditView()
    {
        if (!OperatingSystem.IsWindows())
            return;

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
                    s.AddSingleton<FileObserverViewModel>();
                    s.AddSingleton<FileObserverView>();
                    s.AddTransient<FileObserverEditServiceViewModel>();
                    s.AddTransient<FileObserverEditServiceView>();
                    s.AddTransient<FileObserverAdvancedConfigViewModel>();
                    s.AddTransient<FileObserverAdvancedConfigView>();
                })
                .Build();
            var prop = typeof(App).GetProperty("AppHost");
            var setter = prop!.GetSetMethod(true);
            setter!.Invoke(null, new object[] { host });

            var service = new ServiceViewModel
            {
                DisplayName = "File Observer - Test",
                ServiceType = "File Observer",
                FileObserverOptions = new FileObserverServiceOptions { FilePath = "path" }
            };

            var view = new MainView(mainVm);
            var border = new Border { DataContext = service };
            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = Border.MouseLeftButtonDownEvent,
                ClickCount = 2
            };
            var method = typeof(MainView).GetMethod("ServiceItem_MouseLeftButtonDown", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method!.Invoke(view, new object[] { border, args });

            view.ContentFrame.Content.Should().BeOfType<FileObserverEditServiceView>();
            ConsoleTestLogger.LogPass();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }
}
