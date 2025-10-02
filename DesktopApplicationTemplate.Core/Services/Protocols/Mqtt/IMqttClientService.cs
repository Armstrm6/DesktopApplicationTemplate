using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;

/// <summary>
/// Defines MQTT client operations for connecting, subscribing, and publishing messages.
/// </summary>
public interface IMqttClientService
{
    /// <summary>
    /// Gets a value indicating whether the MQTT client is currently connected.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Raised when the connection state changes.
    /// </summary>
    event EventHandler<bool>? ConnectionStateChanged;

    /// <summary>
    /// Connects to the MQTT broker using stored or override options.
    /// </summary>
    Task ConnectAsync(MqttServiceOptions? overrideOptions = null, CancellationToken token = default);

    /// <summary>
    /// Subscribes to a topic with the specified quality of service level.
    /// </summary>
    Task<MqttClientSubscribeResult> SubscribeAsync(string topic, MqttQualityOfServiceLevel qos, CancellationToken token = default);

    /// <summary>
    /// Unsubscribes from a topic.
    /// </summary>
    Task<MqttClientUnsubscribeResult> UnsubscribeAsync(string topic, CancellationToken token = default);

    /// <summary>
    /// Publishes a single message to a topic.
    /// </summary>
    Task PublishAsync(string topic, string message, CancellationToken token = default);

    /// <summary>
    /// Publishes multiple messages grouped by endpoint.
    /// </summary>
    Task PublishAsync(IDictionary<string, IEnumerable<string>> endpointMessages, CancellationToken token = default);

    /// <summary>
    /// Disconnects from the MQTT broker if connected.
    /// </summary>
    Task DisconnectAsync(CancellationToken token = default);
}
