using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Models;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace DesktopApplicationTemplate.UI.Services;

/// <summary>
/// Provides MQTT connectivity and tokenized message publishing.
/// </summary>
public class MqttService
{
    private readonly IMqttClient _client;
    private readonly IMessageRoutingService _routingService;
    private readonly ILoggingService _logger;
    private readonly MqttServiceOptions _options;
    private readonly Dictionary<string, TagSubscription> _tagSubscriptions = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttService"/> class.
    /// </summary>
    public MqttService(IOptions<MqttServiceOptions> options, IMessageRoutingService routingService, ILoggingService logger)
        : this(new MqttFactory().CreateMqttClient(), options, routingService, logger)
    {
    }

    internal MqttService(IMqttClient client, IOptions<MqttServiceOptions> options, IMessageRoutingService routingService, ILoggingService logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        _client.ConnectedAsync += _ =>
        {
            _logger.Log("MQTT connected", LogLevel.Debug);
            OnConnectionStateChanged(true);
            return Task.CompletedTask;
        };

        _client.DisconnectedAsync += e =>
        {
            var message = e?.Exception != null ? $"MQTT disconnected: {e.Exception.Message}" : "MQTT disconnected";
            _logger.Log(message, LogLevel.Warning);
            OnConnectionStateChanged(false);
            return Task.CompletedTask;
        };

        _client.ConnectingFailedAsync += e =>
        {
            var message = e?.Exception != null ? $"MQTT connection failed: {e.Exception.Message}" : "MQTT connection failed";
            _logger.Log(message, LogLevel.Error);
            return Task.CompletedTask;
        };
    }

    /// <summary>
    /// Gets a value indicating whether the client is connected.
    /// </summary>
    public bool IsConnected => _client.IsConnected;

    /// <summary>
    /// Raised when the connection state changes.
    /// </summary>
    public event EventHandler<bool>? ConnectionStateChanged;

    private void OnConnectionStateChanged(bool connected) => ConnectionStateChanged?.Invoke(this, connected);

    /// <summary>
    /// Raised when tag subscription metadata changes.
    /// </summary>
    public event EventHandler<TagSubscription>? TagSubscriptionChanged;

    /// <summary>
    /// Gets the current set of tag subscriptions.
    /// </summary>
    public IReadOnlyCollection<TagSubscription> TagSubscriptions => _tagSubscriptions.Values;

    /// <summary>
    /// Adds or updates a tag subscription and notifies listeners.
    /// </summary>
    /// <param name="subscription">The subscription to upsert.</param>
    public void UpdateTagSubscription(TagSubscription subscription)
    {
        if (subscription is null) throw new ArgumentNullException(nameof(subscription));
        _tagSubscriptions[subscription.Topic] = subscription;
        TagSubscriptionChanged?.Invoke(this, subscription);
    }

    /// <summary>
    /// Connects to the MQTT broker using configured or override options.
    /// </summary>
    public async Task ConnectAsync(MqttServiceOptions? overrideOptions = null, CancellationToken token = default)
    {
        var opts = overrideOptions ?? _options;
        if (string.IsNullOrWhiteSpace(opts.Host))
            throw new ArgumentException("Host cannot be null or whitespace.", nameof(overrideOptions));

        _logger.Log("MQTT connect start", LogLevel.Debug);

        if (_client.IsConnected)
        {
            _logger.Log("Disconnecting existing MQTT connection", LogLevel.Debug);
            await _client.DisconnectAsync(cancellationToken: token).ConfigureAwait(false);
        }

        _logger.Log(
            $"MQTT options: Host={opts.Host}, Port={opts.Port}, ClientId={opts.ClientId}, " +
            $"WillTopic={opts.WillTopic}, WillQoS={opts.WillQualityOfService}, WillRetain={opts.WillRetain}, " +
            $"KeepAlive={opts.KeepAliveSeconds}, CleanSession={opts.CleanSession}, ReconnectDelay={opts.ReconnectDelay}",
            LogLevel.Debug);

        var builder = new MqttClientOptionsBuilder()
            .WithClientId(opts.ClientId)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(opts.KeepAliveSeconds))
            .WithCleanSession(opts.CleanSession);

        if (!string.IsNullOrEmpty(opts.WillTopic))
        {
            builder = builder
                .WithWillTopic(opts.WillTopic)
                .WithWillPayload(opts.WillPayload ?? string.Empty)
                .WithWillQualityOfServiceLevel(opts.WillQualityOfService)
                .WithWillRetain(opts.WillRetain);
        }

        if (opts.ConnectionType == MqttConnectionType.WebSocket || opts.ConnectionType == MqttConnectionType.WebSocketTls)
        {
            var path = string.IsNullOrWhiteSpace(opts.WebSocketPath) ? string.Empty : opts.WebSocketPath;
            var scheme = opts.ConnectionType == MqttConnectionType.WebSocketTls ? "wss" : "ws";
            builder = builder.WithWebSocketServer(o =>
            {
                o.WithUri($"{scheme}://{opts.Host}:{opts.Port}{path}");
            });
        }
        else
        {
            builder = builder.WithTcpServer(opts.Host, opts.Port);
        }

        if (!string.IsNullOrEmpty(opts.Username))
        {
            builder = builder.WithCredentials(opts.Username, opts.Password);
        }

        if (opts.ConnectionType == MqttConnectionType.MqttTls || opts.ConnectionType == MqttConnectionType.WebSocketTls)
        {
            builder = builder.WithTlsOptions(o =>
            {
                o.UseTls();
                if (opts.ClientCertificate is not null)
                {
                    try
                    {
                        o.WithClientCertificates(new[] { new X509Certificate2(opts.ClientCertificate) });
                    }
                    catch (CryptographicException ex)
                    {
                        _logger.Log($"Invalid client certificate: {ex.Message}", LogLevel.Warning);
                    }
                }
            });
        }

        _logger.Log("MQTT options configured", LogLevel.Debug);
        var mqttOptions = builder.Build();

        while (true)
        {
            try
            {
                await _client.ConnectAsync(mqttOptions, token).ConfigureAwait(false);
                _logger.Log("MQTT connect finished", LogLevel.Debug);
                break;
            }
            catch (Exception ex) when (opts.ReconnectDelay.HasValue && !token.IsCancellationRequested)
            {
                _logger.Log($"MQTT connect failed: {ex.Message}. Retrying in {opts.ReconnectDelay}", LogLevel.Warning);
                await Task.Delay(opts.ReconnectDelay.Value, token).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Subscribes to a topic with the specified quality of service level.
    /// </summary>
    /// <param name="topic">The topic to subscribe to.</param>
    /// <param name="qos">The desired QoS level.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The MQTT subscribe result when successful.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="topic"/> is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the broker rejects the subscription.</exception>
    public async Task<MqttClientSubscribeResult> SubscribeAsync(string topic, MqttQualityOfServiceLevel qos, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic cannot be null or whitespace.", nameof(topic));

        _logger.Log("MQTT subscribe start", LogLevel.Debug);

        var filter = new MqttTopicFilterBuilder()
            .WithTopic(topic)
            .WithQualityOfServiceLevel(qos)
            .Build();
        var options = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(filter)
            .Build();

        var result = await _client.SubscribeAsync(options, token).ConfigureAwait(false);

        var success = true;
        foreach (var item in result.Items)
        {
            if (item.ResultCode != MqttClientSubscribeResultCode.GrantedQoS0 &&
                item.ResultCode != MqttClientSubscribeResultCode.GrantedQoS1 &&
                item.ResultCode != MqttClientSubscribeResultCode.GrantedQoS2)
            {
                success = false;
                break;
            }
        }

        if (!success)
        {
            var codes = string.Join(',', result.Items.Select(i => i.ResultCode));
            _logger.Log($"MQTT subscribe failed: {codes}", LogLevel.Error);
            throw new InvalidOperationException($"Subscription failed: {codes}");
        }

        _logger.Log("MQTT subscribe finished", LogLevel.Debug);
        return result;
    }

    /// <summary>
    /// Unsubscribes from a topic.
    /// </summary>
    /// <param name="topic">The topic to unsubscribe from.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The MQTT unsubscribe result when successful.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="topic"/> is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the broker rejects the request.</exception>
    public async Task<MqttClientUnsubscribeResult> UnsubscribeAsync(string topic, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic cannot be null or whitespace.", nameof(topic));

        _logger.Log("MQTT unsubscribe start", LogLevel.Debug);

        var options = new MqttClientUnsubscribeOptionsBuilder()
            .WithTopicFilter(topic)
            .Build();

        var result = await _client.UnsubscribeAsync(options, token).ConfigureAwait(false);

        _logger.Log("MQTT unsubscribe finished", LogLevel.Debug);
        return result;
    }

    /// <summary>
    /// Publishes a single message to an endpoint.
    /// </summary>
    public async Task PublishAsync(string topic, string message, CancellationToken token = default)
    {
        if (topic is null) throw new ArgumentNullException(nameof(topic));
        if (message is null) throw new ArgumentNullException(nameof(message));

        _logger.Log("MQTT publish start", LogLevel.Debug);
        var payload = _routingService.ResolveTokens(message);
        var msg = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .Build();
        await _client.PublishAsync(msg, token).ConfigureAwait(false);
        _logger.Log("MQTT publish finished", LogLevel.Debug);
    }

    /// <summary>
    /// Publishes multiple messages per endpoint.
    /// </summary>
    public async Task PublishAsync(IDictionary<string, IEnumerable<string>> endpointMessages, CancellationToken token = default)
    {
        if (endpointMessages is null) throw new ArgumentNullException(nameof(endpointMessages));
        foreach (var pair in endpointMessages)
        {
            foreach (var msg in pair.Value)
            {
                await PublishAsync(pair.Key, msg, token).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Disconnects from the broker if connected.
    /// </summary>
    public async Task DisconnectAsync(CancellationToken token = default)
    {
        if (_client.IsConnected)
        {
            await _client.DisconnectAsync(cancellationToken: token).ConfigureAwait(false);
        }
    }
}
