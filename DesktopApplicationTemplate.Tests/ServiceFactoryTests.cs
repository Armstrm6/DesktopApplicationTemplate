using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI;
using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class ServiceFactoryTests
{
    static ServiceFactoryTests()
    {
        // Ensure the application's service provider is built for factory resolution
        _ = new App();
    }

    public static IEnumerable<object[]> FactoryTypes() => new[]
    {
        new object[] { ServiceType.Mqtt },
        new object[] { ServiceType.Ftp },
        new object[] { ServiceType.Http },
        new object[] { ServiceType.Tcp },
        new object[] { ServiceType.Hid },
        new object[] { ServiceType.Scp },
        new object[] { ServiceType.Csv },
        new object[] { ServiceType.FileObserver },
        new object[] { ServiceType.Heartbeat }
    };

    [WindowsTheory]
    [MemberData(nameof(FactoryTypes))]
    public void Create_ReturnsServicePage(ServiceType type)
    {
        var provider = App.AppHost.Services;
        var registry = provider.GetRequiredService<IServiceUiRegistry<ServiceListModel, Page>>();

        object ctx = type switch
        {
            ServiceType.Mqtt => new ServiceFactoryOptions<MqttServiceOptions>("mqtt", new MqttServiceOptions
            {
                Host = "localhost",
                Port = 1883,
                ClientId = "client"
            }),
            ServiceType.Ftp => new ServiceFactoryOptions<FtpServerOptions>("ftp", new FtpServerOptions
            {
                RootPath = "."
            }),
            ServiceType.Http => new ServiceFactoryOptions<HttpServiceOptions>("http", new HttpServiceOptions
            {
                BaseUrl = "http://localhost"
            }),
            ServiceType.Tcp => new ServiceFactoryOptions<TcpServiceOptions>("tcp", new TcpServiceOptions
            {
                Host = "localhost",
                Port = 1
            }),
            ServiceType.Hid => new ServiceFactoryOptions<HidServiceOptions>("hid", new HidServiceOptions()),
            ServiceType.Scp => new ServiceFactoryOptions<ScpServiceOptions>("scp", new ScpServiceOptions
            {
                Host = "host",
                Username = "user",
                Password = "pwd",
                LocalPath = "local",
                RemotePath = "remote"
            }),
            ServiceType.Csv => new ServiceFactoryOptions<CsvServiceOptions>("csv", new CsvServiceOptions
            {
                OutputPath = "."
            }),
            ServiceType.FileObserver => new ServiceFactoryOptions<FileObserverServiceOptions>("fo", new FileObserverServiceOptions
            {
                FilePath = "."
            }),
            ServiceType.Heartbeat => new ServiceFactoryOptions<HeartbeatServiceOptions>("hb", new HeartbeatServiceOptions
            {
                BaseMessage = "ping"
            }),
            _ => throw new NotSupportedException()
        };

        var created = registry.TryCreateService(type, provider, ctx, out var svc);
        Assert.True(created);
        Assert.NotNull(svc);

        var mainView = provider.GetRequiredService<MainView>();
        var page = mainView.GetOrCreateServicePage(svc!);
        Assert.NotNull(page);
    }
}
