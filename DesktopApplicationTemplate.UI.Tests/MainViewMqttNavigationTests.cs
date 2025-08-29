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

public class MainViewMqttNavigationTests
{
    [WindowsFact]
    public void CreateMqttService_Advanced_Back_ReturnsToCreateView()
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
                    s.AddTransient<MqttCreateServiceViewModel>();
                    s.AddTransient<MqttCreateServiceView>();
                    s.AddTransient<MqttAdvancedConfigViewModel>();
                    s.AddTransient<MqttAdvancedConfigView>();
                })
                .Build();
            var prop = typeof(App).GetProperty("AppHost");
            var setter = prop!.GetSetMethod(true);
            setter!.Invoke(null, new object[] { host });

            var view = new MainView(mainVm);
            var method = typeof(MainView).GetMethod("NavigateToMqtt", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method!.Invoke(view, new object[] { "Test" });

            view.ContentFrame.Content.Should().BeOfType<MqttCreateServiceView>();
            var createView = (MqttCreateServiceView)view.ContentFrame.Content;
            var vm = (MqttCreateServiceViewModel)createView.DataContext;
            vm.AdvancedConfigCommand.Execute(null);

            view.ContentFrame.Content.Should().BeOfType<MqttAdvancedConfigView>();
            var advView = (MqttAdvancedConfigView)view.ContentFrame.Content;
            var advVm = (MqttAdvancedConfigViewModel)advView.DataContext;
            advVm.BackCommand.Execute(null);

            view.ContentFrame.Content.Should().BeOfType<MqttCreateServiceView>();
            ConsoleTestLogger.LogPass();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }

    [WindowsFact]
    public void EditMqttService_ShowsEditView()
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
                    s.AddSingleton<MqttTagSubscriptionsViewModel>();
                    s.AddSingleton<MqttTagSubscriptionsView>();
                    s.AddTransient<MqttEditServiceViewModel>();
                    s.AddTransient<MqttEditServiceView>();
                    s.AddTransient<MqttAdvancedConfigViewModel>();
                    s.AddTransient<MqttAdvancedConfigView>();
                    s.Configure<MqttServiceOptions>(o =>
                    {
                        o.Host = "h";
                        o.Port = 1;
                        o.ClientId = "c";
                    });
                })
                .Build();
            var prop = typeof(App).GetProperty("AppHost");
            var setter = prop!.GetSetMethod(true);
            setter!.Invoke(null, new object[] { host });

            var service = new ServiceViewModel
            {
                DisplayName = "MQTT - Test",
                ServiceType = "MQTT"
            };

            var view = new MainView(mainVm);
            var method = typeof(MainView).GetMethod("OnEditRequested", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method!.Invoke(view, new object[] { service });

            view.ContentFrame.Content.Should().BeOfType<MqttEditServiceView>();
            ConsoleTestLogger.LogPass();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }
}
