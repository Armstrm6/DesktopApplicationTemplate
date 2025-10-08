using System;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;

/// <summary>
/// Factory for creating isolated <see cref="IMqttClientService"/> instances.
/// </summary>
public interface IMqttClientServiceFactory
{
    /// <summary>
    /// Creates a new MQTT client service bound to the specified options instance.
    /// </summary>
    /// <param name="options">The options to associate with the client.</param>
    /// <returns>A configured MQTT client service.</returns>
    IMqttClientService Create(MqttServiceOptions options);
}

/// <summary>
/// Default implementation of <see cref="IMqttClientServiceFactory"/>.
/// </summary>
public sealed class MqttClientServiceFactory : IMqttClientServiceFactory
{
    private readonly IMessageRoutingService _routingService;
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttClientServiceFactory"/> class.
    /// </summary>
    public MqttClientServiceFactory(IMessageRoutingService routingService, ILoggingService logger)
    {
        _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public IMqttClientService Create(MqttServiceOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var client = new MqttFactory().CreateMqttClient();
        return new MqttService(client, Options.Create(options), _routingService, _logger);
    }
}
