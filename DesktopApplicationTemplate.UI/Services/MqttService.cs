
using MQTTnet;
using MQTTnet.Client;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using Microsoft.Extensions.Options;
using DesktopApplicationTemplate.UI.Models;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Provides basic MQTT operations.
    /// </summary>
    public class MqttService
    {
        private readonly IMqttClient _client;
        private readonly ILoggingService _logger;
        private readonly MqttServiceOptions _options;
        private readonly IMessageRoutingService _routingService;
        private readonly ILoggingService? _logger;
        private readonly HashSet<string> _subscriptions = new();
        private MqttClientOptions? _clientOptions;
        private MqttServiceOptions? _serviceOptions;
        private Func<MqttClientDisconnectedEventArgs, Task>? _reconnectHandler;
        public MqttService(IOptions<MqttServiceOptions> options, ILoggingService logger)

        /// <summary>
        /// Connection options in use by the service.
        /// </summary>
        public MqttServiceOptions Options { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttService"/> class.
        /// </summary>
        public MqttService(MqttServiceOptions options, ILoggingService? logger = null)
        {
            _options = options.Value;
            _logger = logger;
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();
        }



        internal MqttService(IMqttClient client, MqttServiceOptions options, ILoggingService? logger = null)
        {
            _client = client;
            Options = options;
            _logger = logger;
            _options = options.Value;
        }

        public virtual async Task ConnectAsync(string host, int port, string clientId, string? user, string? pass, bool useTls = false, CancellationToken token = default)
        {
            _logger.Log("MqttService connect start", LogLevel.Debug);
            var builder = new MqttClientOptionsBuilder()
                .WithTcpServer(host ?? _options.Host, port ?? _options.Port)
                .WithClientId(clientId ?? _options.ClientId);

            var username = user ?? _options.Username;
            var password = pass ?? _options.Password;
            if (!string.IsNullOrEmpty(username))
                builder = builder.WithCredentials(username, password);

            _logger.Log($"Connecting to MQTT {host ?? _options.Host}:{port ?? _options.Port}", LogLevel.Debug);
            await _client.ConnectAsync(builder.Build()).ConfigureAwait(false);
            _logger.Log("MQTT connected", LogLevel.Debug);
            _logger.Log("MqttService connect finished", LogLevel.Debug);
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
        /// Connects to the MQTT broker using the configured options.
        /// </summary>
        public async Task ConnectAsync()
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(options.Host))
                throw new ArgumentException("Host cannot be null or whitespace.", nameof(options));

            _logger?.Log("MqttService connect start", LogLevel.Debug);

            if (_client.IsConnected)
            {
                _logger?.Log("Disconnecting existing MQTT connection", LogLevel.Debug);
                await _client.DisconnectAsync();
            }

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(host, port)
                .WithClientId(clientId);

            if (!string.IsNullOrEmpty(user))
            {
                options = options.WithCredentials(user, pass);
            }

            if (useTls)
            {
                options = options.WithTlsOptions(o => o.UseTls());
            }

            _logger?.Log($"Connecting to MQTT {host}:{port}", LogLevel.Debug);

            if (_client.IsConnected)
            {
                await _client.DisconnectAsync(cancellationToken: token).ConfigureAwait(false);
            }

            await _client.ConnectAsync(options.Build(), token).ConfigureAwait(false);
            var builder = new MqttClientOptionsBuilder()
                .WithTcpServer(Options.Host, Options.Port)
                .WithClientId(Options.ClientId);

            if (!string.IsNullOrEmpty(Options.Username))
                builder = builder.WithCredentials(Options.Username, Options.Password);

            _logger?.Log($"Connecting to MQTT {Options.Host}:{Options.Port}", LogLevel.Debug);
            await _client.ConnectAsync(builder.Build()).ConfigureAwait(false);
            _logger?.Log("MQTT connected", LogLevel.Debug);
            _logger?.Log("MqttService connect finished", LogLevel.Debug);
            OnConnectionStateChanged(true);
        }

        public virtual async Task SubscribeAsync(IEnumerable<string> topics)
        {
            foreach (var t in topics)
            {
                _logger?.Log($"Subscribing to {t}", LogLevel.Debug);

                await _client.SubscribeAsync(t).ConfigureAwait(false);
            }
        }

        public virtual async Task PublishAsync(string topic, string message)
        {

            _logger?.Log("MqttService publish start", LogLevel.Debug);
            _logger?.Log($"Publishing to {topic}", LogLevel.Debug);
            var resolved = _routingService.ResolveTokens(message);
            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(resolved)
                .Build();
            await _client.PublishAsync(msg).ConfigureAwait(false);
            _logger.Log("Publish complete", LogLevel.Debug);
            _logger.Log("MqttService publish finished", LogLevel.Debug);

        }

        public async Task PublishAsync(string topic, IEnumerable<string> messages)
        {
            foreach (var msg in messages)
            {
                await PublishAsync(topic, msg).ConfigureAwait(false);
            }
        }

        public async Task PublishAsync(IDictionary<string, IEnumerable<string>> endpointMessages)
        {
            foreach (var pair in endpointMessages)
            {
                await PublishAsync(pair.Key, pair.Value).ConfigureAwait(false);
            }
        }

        public virtual Task DisconnectAsync(CancellationToken token = default)
        {
            return _client.DisconnectAsync(cancellationToken: token);
        }
    }
}
