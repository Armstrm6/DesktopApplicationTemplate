using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI;
using DesktopApplicationTemplate.UI.Factories;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.Core.Services;
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
        var catalog = provider.GetRequiredService<IServiceCatalog>();
        var factory = provider.GetKeyedService<IServiceFactory>(type);
        Assert.NotNull(factory);

        _ = catalog.TryGetByLegacyType(type, out var descriptor);
        if (descriptor is null && catalog.LegacyMap.TryGetValue(type, out var legacyId) && catalog.TryGetById(legacyId, out var mapped))
        {
            descriptor = mapped;
        }

        var descriptorId = descriptor?.Id ?? (catalog.LegacyMap.TryGetValue(type, out var fallbackId) ? fallbackId : type.ToLegacyString());

        var (serviceName, payload) = type switch
        {
            ServiceType.Mqtt => ("mqtt", (object)new MqttServiceOptions
            {
                Host = "localhost",
                Port = 1883,
                ClientId = "client"
            }),
            ServiceType.Ftp => ("ftp", (object)new FtpServerOptions
            {
                RootPath = "."
            }),
            ServiceType.Http => ("http", (object)new HttpServiceOptions
            {
                BaseUrl = "http://localhost"
            }),
            ServiceType.Tcp => ("tcp", (object)new TcpServiceOptions
            {
                Host = "localhost",
                Port = 1
            }),
            ServiceType.Hid => ("hid", (object)new HidServiceOptions()),
            ServiceType.Scp => ("scp", (object)new ScpServiceOptions
            {
                Host = "host",
                Username = "user",
                Password = "pwd",
                LocalPath = "local",
                RemotePath = "remote"
            }),
            ServiceType.Csv => ("csv", (object)new CsvServiceOptions
            {
                OutputPath = "."
            }),
            ServiceType.FileObserver => ("fo", (object)new FileObserverServiceOptions
            {
                FilePath = "."
            }),
            ServiceType.Heartbeat => ("hb", (object)new HeartbeatServiceOptions
            {
                BaseMessage = "ping"
            }),
            _ => throw new NotSupportedException()
        };

        var context = new ServiceFactoryContext(descriptorId, serviceName, payload, descriptor);

        var svc = factory!.Create(context);
        Assert.NotNull(svc.ServicePage);
    }
}
