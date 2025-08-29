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
using System.Windows.Controls;
using System.Windows.Input;

namespace DesktopApplicationTemplate.Tests;

public class MainViewScpNavigationTests
{
    [WindowsFact]
    public void EditScpService_ShowsEditView()
    {
        ApplicationResourceHelper.RunOnDispatcher(() =>
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
                    s.AddSingleton<ScpServiceViewModel>();
                    s.AddSingleton<SCPServiceView>();
                    s.AddTransient<ScpEditServiceViewModel>();
                    s.AddTransient<ScpEditServiceView>();
                    s.AddTransient<ScpAdvancedConfigViewModel>();
                    s.AddTransient<ScpAdvancedConfigView>();
                    s.AddOptions<ScpServiceOptions>();
                })
                .Build();
            var prop = typeof(App).GetProperty("AppHost");
            var setter = prop!.GetSetMethod(true);
            setter!.Invoke(null, new object[] { host });

            var service = new ServiceViewModel
            {
                DisplayName = "SCP - Test",
                ServiceType = "SCP",
                ScpOptions = new ScpServiceOptions { Host = "h" }
            };

            var view = new MainView(mainVm);
            var method = typeof(MainView).GetMethod("OnEditRequested", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method!.Invoke(view, new object[] { service });

            view.ContentFrame.Content.Should().BeOfType<ScpEditServiceView>();
            ConsoleTestLogger.LogPass();
        });
    }

    [WindowsFact]
    public void DoubleClickScpService_OpensEditView()
    {
        ApplicationResourceHelper.RunOnDispatcher(() =>
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
                    s.AddSingleton<ScpServiceViewModel>();
                    s.AddSingleton<SCPServiceView>();
                    s.AddTransient<ScpEditServiceViewModel>();
                    s.AddTransient<ScpEditServiceView>();
                    s.AddTransient<ScpAdvancedConfigViewModel>();
                    s.AddTransient<ScpAdvancedConfigView>();
                    s.AddOptions<ScpServiceOptions>();
                })
                .Build();
            var prop = typeof(App).GetProperty("AppHost");
            var setter = prop!.GetSetMethod(true);
            setter!.Invoke(null, new object[] { host });

            var service = new ServiceViewModel
            {
                DisplayName = "SCP - Test",
                ServiceType = "SCP",
                ScpOptions = new ScpServiceOptions { Host = "h" }
            };

            var view = new MainView(mainVm);
            var border = new Border { DataContext = service };
            var args = MouseButtonEventArgsFactory.Create(MouseButton.Left, 2);
            args.RoutedEvent = Border.MouseLeftButtonDownEvent;
            var method = typeof(MainView).GetMethod("ServiceItem_MouseLeftButtonDown", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method!.Invoke(view, new object[] { border, args });

            view.ContentFrame.Content.Should().BeOfType<ScpEditServiceView>();
            ConsoleTestLogger.LogPass();
        });
    }
}
