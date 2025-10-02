using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;

/// <summary>
/// Extension methods for registering MQTT client services.
/// </summary>
public static class MqttServiceCollectionExtensions
{
    /// <summary>
    /// Registers the MQTT client service implementation.
    /// </summary>
    public static IServiceCollection AddMqttClientService(this IServiceCollection services)
    {
        services.AddSingleton<IMqttClientService, MqttService>();
        return services;
    }
}
