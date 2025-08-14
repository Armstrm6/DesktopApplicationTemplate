using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;

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
            OnConnectionStateChanged(false);
        }

        var builder = new MqttClientOptionsBuilder().WithClientId(opts.ClientId);
        if (opts.ConnectionType == MqttConnectionType.WebSocket)
        {
            builder = builder.WithWebSocketServer($"{opts.Host}:{opts.Port}");
        }
        else
        {
            builder = builder.WithTcpServer(opts.Host, opts.Port);
        }

        if (!string.IsNullOrEmpty(opts.Username))
            builder = builder.WithCredentials(opts.Username, opts.Password);

        if (opts.UseTls)
        {
            builder = builder.WithTlsOptions(o =>
            {
                o.UseTls();
                if (opts.ClientCertificate is not null)
                {
                    o.WithClientCertificates(new X509Certificate2(opts.ClientCertificate));
                }
            });
        }

        await _client.ConnectAsync(builder.Build(), token).ConfigureAwait(false);
        _logger.Log("MQTT connected", LogLevel.Debug);
        OnConnectionStateChanged(true);
        _logger.Log("MQTT connect finished", LogLevel.Debug);
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
            OnConnectionStateChanged(false);
        }
    }
}

