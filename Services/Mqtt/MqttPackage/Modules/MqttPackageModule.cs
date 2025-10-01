using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Mqtt.Descriptors;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Services.Mqtt.Modules;

public sealed class MqttPackageModule : IServiceModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<MqttServiceDescriptor>();
        services.AddSingleton<IServiceDescriptor>(sp => sp.GetRequiredService<MqttServiceDescriptor>());
    }

    public IEnumerable<IServiceDescriptor> DescribeServices()
    {
        yield return new MqttServiceDescriptor();
    }
}
