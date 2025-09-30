using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Serialization;
using DesktopApplicationTemplate.Services.Common.Descriptors;
using DesktopApplicationTemplate.UI.Factories;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using DesktopApplicationTemplate.UI.Navigation;
using System;

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
                        sp => ResolveFactory(sp, MqttServiceDescriptor.DescriptorId),
                        MqttServiceDescriptor.DescriptorId)
            });

        yield return new HttpServiceDescriptor(
            new JsonServiceOptionsSerializer<HttpServiceOptions>(),
            new[]
            {
                    ServiceFactoryBinding.Create(
                        ServiceFactoryKind.UserInterface,
                        typeof(IServiceFactory),
                        sp => ResolveFactory(sp, HttpServiceDescriptor.DescriptorId),
                        HttpServiceDescriptor.DescriptorId)
            });

        yield return new FtpServiceDescriptor(
            new JsonServiceOptionsSerializer<FtpServerOptions>(),
            new[]
            {
                    ServiceFactoryBinding.Create(
                        ServiceFactoryKind.UserInterface,
                        typeof(IServiceFactory),
                        sp => ResolveFactory(sp, FtpServiceDescriptor.DescriptorId),
                        FtpServiceDescriptor.DescriptorId)
            });

        yield return new HidServiceDescriptor(
            new JsonServiceOptionsSerializer<HidServiceOptions>(),
            new[]
            {
                    ServiceFactoryBinding.Create(
                        ServiceFactoryKind.UserInterface,
                        typeof(IServiceFactory),
                        sp => ResolveFactory(sp, HidServiceDescriptor.DescriptorId),
                        HidServiceDescriptor.DescriptorId)
            });

        yield return new CsvServiceDescriptor(
            new JsonServiceOptionsSerializer<CsvServiceOptions>(),
            new[]
            {
                    ServiceFactoryBinding.Create(
                        ServiceFactoryKind.UserInterface,
                        typeof(IServiceFactory),
                        sp => ResolveFactory(sp, CsvServiceDescriptor.DescriptorId),
                        CsvServiceDescriptor.DescriptorId)
            });

        yield return new FileObserverServiceDescriptor(
            new JsonServiceOptionsSerializer<FileObserverServiceOptions>(),
            new[]
            {
                    ServiceFactoryBinding.Create(
                        ServiceFactoryKind.UserInterface,
                        typeof(IServiceFactory),
                        sp => ResolveFactory(sp, FileObserverServiceDescriptor.DescriptorId),
                        FileObserverServiceDescriptor.DescriptorId)
            });

        yield return new ScpServiceDescriptor(
            new JsonServiceOptionsSerializer<ScpServiceOptions>(),
            new[]
            {
                    ServiceFactoryBinding.Create(
                        ServiceFactoryKind.UserInterface,
                        typeof(IServiceFactory),
                        sp => ResolveFactory(sp, ScpServiceDescriptor.DescriptorId),
                        ScpServiceDescriptor.DescriptorId)
            });

        yield return new TcpServiceDescriptor(
            new JsonServiceOptionsSerializer<TcpServiceOptions>(),
            new[]
            {
                    ServiceFactoryBinding.Create(
                        ServiceFactoryKind.UserInterface,
                        typeof(IServiceFactory),
                        sp => ResolveFactory(sp, TcpServiceDescriptor.DescriptorId),
                        TcpServiceDescriptor.DescriptorId)
            });

        yield return new HeartbeatServiceDescriptor(
            new JsonServiceOptionsSerializer<HeartbeatServiceOptions>(),
            new[]
            {
                    ServiceFactoryBinding.Create(
                        ServiceFactoryKind.UserInterface,
                        typeof(IServiceFactory),
                        sp => ResolveFactory(sp, HeartbeatServiceDescriptor.DescriptorId),
                        HeartbeatServiceDescriptor.DescriptorId)
            });
    }

    private static object ResolveFactory(IServiceProvider services, string descriptorId)
    {
        var registry = services.GetRequiredService<IServiceUiRegistry>();
        if (!registry.Factories.TryGetValue(descriptorId, out var factory))
        {
            throw new InvalidOperationException($"No UI factory registered for descriptor '{descriptorId}'.");
        }

        return factory();
    }
}
