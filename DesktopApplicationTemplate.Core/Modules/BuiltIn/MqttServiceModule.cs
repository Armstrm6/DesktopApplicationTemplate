using DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;
using DesktopApplicationTemplate.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Core.Modules.BuiltIn;

/// <summary>
/// Registers the built-in MQTT client services and descriptor.
/// </summary>
public sealed class MqttServiceModule : BuiltInServiceModule<MqttServiceDescriptor>
{
    public override ServiceType Type => ServiceType.Mqtt;

    public override void RegisterServices(IServiceCollection services)
    {
        base.RegisterServices(services);
        services.AddMqttClientService();
    }
}
