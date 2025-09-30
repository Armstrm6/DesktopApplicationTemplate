using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Serialization;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services.Common.Descriptors;
using DesktopApplicationTemplate.UI.Factories;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.UI.Modules;

public sealed class UiServiceModule : IServiceModule
{
    public void RegisterServices(IServiceCollection services)
    {
        // UI service registrations remain in App.xaml configuration for now.
    }

    public IEnumerable<IServiceDescriptor> DescribeServices()
    {
        yield return new MqttServiceDescriptor(
            new JsonServiceOptionsSerializer<MqttServiceOptions>(),
            new[]
            {
                ServiceFactoryBinding.Create(
                    ServiceFactoryKind.UserInterface,
                    typeof(IServiceFactory),
                    sp => sp.GetRequiredKeyedService<IServiceFactory>(ServiceType.Mqtt),
                    MqttServiceDescriptor.DescriptorId)
            });

        yield return new HttpServiceDescriptor(
            new JsonServiceOptionsSerializer<HttpServiceOptions>(),
            new[]
            {
                ServiceFactoryBinding.Create(
                    ServiceFactoryKind.UserInterface,
                    typeof(IServiceFactory),
                    sp => sp.GetRequiredKeyedService<IServiceFactory>(ServiceType.Http),
                    HttpServiceDescriptor.DescriptorId)
            });

        yield return new FtpServiceDescriptor(
            new JsonServiceOptionsSerializer<FtpServerOptions>(),
            new[]
            {
                ServiceFactoryBinding.Create(
                    ServiceFactoryKind.UserInterface,
                    typeof(IServiceFactory),
                    sp => sp.GetRequiredKeyedService<IServiceFactory>(ServiceType.Ftp),
                    FtpServiceDescriptor.DescriptorId)
            });

        yield return new HidServiceDescriptor(
            new JsonServiceOptionsSerializer<HidServiceOptions>(),
            new[]
            {
                ServiceFactoryBinding.Create(
                    ServiceFactoryKind.UserInterface,
                    typeof(IServiceFactory),
                    sp => sp.GetRequiredKeyedService<IServiceFactory>(ServiceType.Hid),
                    HidServiceDescriptor.DescriptorId)
            });

        yield return new CsvServiceDescriptor(
            new JsonServiceOptionsSerializer<CsvServiceOptions>(),
            new[]
            {
                ServiceFactoryBinding.Create(
                    ServiceFactoryKind.UserInterface,
                    typeof(IServiceFactory),
                    sp => sp.GetRequiredKeyedService<IServiceFactory>(ServiceType.Csv),
                    CsvServiceDescriptor.DescriptorId)
            });

        yield return new FileObserverServiceDescriptor(
            new JsonServiceOptionsSerializer<FileObserverServiceOptions>(),
            new[]
            {
                ServiceFactoryBinding.Create(
                    ServiceFactoryKind.UserInterface,
                    typeof(IServiceFactory),
                    sp => sp.GetRequiredKeyedService<IServiceFactory>(ServiceType.FileObserver),
                    FileObserverServiceDescriptor.DescriptorId)
            });

        yield return new ScpServiceDescriptor(
            new JsonServiceOptionsSerializer<ScpServiceOptions>(),
            new[]
            {
                ServiceFactoryBinding.Create(
                    ServiceFactoryKind.UserInterface,
                    typeof(IServiceFactory),
                    sp => sp.GetRequiredKeyedService<IServiceFactory>(ServiceType.Scp),
                    ScpServiceDescriptor.DescriptorId)
            });

        yield return new TcpServiceDescriptor(
            new JsonServiceOptionsSerializer<TcpServiceOptions>(),
            new[]
            {
                ServiceFactoryBinding.Create(
                    ServiceFactoryKind.UserInterface,
                    typeof(IServiceFactory),
                    sp => sp.GetRequiredKeyedService<IServiceFactory>(ServiceType.Tcp),
                    TcpServiceDescriptor.DescriptorId)
            });

        yield return new HeartbeatServiceDescriptor(
            new JsonServiceOptionsSerializer<HeartbeatServiceOptions>(),
            new[]
            {
                ServiceFactoryBinding.Create(
                    ServiceFactoryKind.UserInterface,
                    typeof(IServiceFactory),
                    sp => sp.GetRequiredKeyedService<IServiceFactory>(ServiceType.Heartbeat),
                    HeartbeatServiceDescriptor.DescriptorId)
            });
    }
}
