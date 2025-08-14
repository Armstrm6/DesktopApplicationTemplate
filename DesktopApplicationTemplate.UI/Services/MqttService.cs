using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace DesktopApplicationTemplate.UI.Services
{
    public class MqttService
    {
        private readonly IMqttClient _client;
        private readonly ILoggingService? _logger;
        private readonly HashSet<string> _subscriptions = new();
        private MqttClientOptions? _clientOptions;
        private MqttServiceOptions? _serviceOptions;
        private Func<MqttClientDisconnectedEventArgs, Task>? _reconnectHandler;

        public MqttService(ILoggingService? logger = null)
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();
            _logger = logger;
        }

        internal MqttService(IMqttClient client, ILoggingService? logger = null)
        {
            _client = client;
            _logger = logger;
        }

        public async Task ConnectAsync(MqttServiceOptions options, CancellationToken cancellationToken = default)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(options.Host))
                throw new ArgumentException("Host cannot be null or whitespace.", nameof(options));

            _logger?.Log("MqttService connect start", LogLevel.Debug);

            await DisconnectAsync(cancellationToken).ConfigureAwait(false);

            _serviceOptions = options;

            var builder = new MqttClientOptionsBuilder()
                .WithClientId(options.ClientId)
                .WithCleanSession(options.CleanSession)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(options.KeepAliveSeconds));

            if (options.ConnectionType == MqttConnectionType.WebSocket)
            {
                var scheme = options.UseTls ? "wss" : "ws";
                builder.WithWebSocketServer(p => p.WithUri($"{scheme}://{options.Host}:{options.Port}"));
            }
            else
            {
                builder.WithTcpServer(options.Host, options.Port);
                if (options.UseTls)
                {
                    builder.WithTlsOptions(o =>
                    {
                        o.UseTls();
                        if (options.ClientCertificate is not null)
                        {
                            o.WithClientCertificates(new[] { new X509Certificate2(options.ClientCertificate) });
                        }
                    });
                }
            }

            if (!string.IsNullOrEmpty(options.Username))
            {
                builder.WithCredentials(options.Username, options.Password);
            }

            if (!string.IsNullOrWhiteSpace(options.WillTopic))
            {
                builder.WithWillTopic(options.WillTopic)
                       .WithWillPayload(options.WillPayload ?? string.Empty)
                       .WithWillQualityOfServiceLevel(options.WillQualityOfService);
                if (options.WillRetain)
                {
                    builder.WithWillRetain();
                }
            }

            _clientOptions = builder.Build();

            if (_reconnectHandler is not null)
            {
                _client.DisconnectedAsync -= _reconnectHandler;
            }

            if (options.ReconnectDelay.HasValue)
            {
                _reconnectHandler = async e =>
                {
                    _logger?.Log("MqttService reconnect start", LogLevel.Debug);
                    await Task.Delay(options.ReconnectDelay.Value, CancellationToken.None).ConfigureAwait(false);
                    try
                    {
                        await _client.ConnectAsync(_clientOptions!, CancellationToken.None).ConfigureAwait(false);
                        if (_subscriptions.Count > 0)
                        {
                            var subscribeBuilder = new MqttClientSubscribeOptionsBuilder();
                            foreach (var t in _subscriptions)
                            {
                                subscribeBuilder.WithTopicFilter(f => f.WithTopic(t));
                            }
                            await _client.SubscribeAsync(subscribeBuilder.Build(), CancellationToken.None).ConfigureAwait(false);
                        }
                        _logger?.Log("MqttService reconnect finished", LogLevel.Debug);
                    }
                    catch (Exception ex)
                    {
                        _logger?.Log($"MqttService reconnect error: {ex.Message}", LogLevel.Error);
                    }
                };
                _client.DisconnectedAsync += _reconnectHandler;
            }

            try
            {
                await _client.ConnectAsync(_clientOptions, cancellationToken).ConfigureAwait(false);
                _logger?.Log("MqttService connect finished", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _logger?.Log($"MqttService connect error: {ex.Message}", LogLevel.Error);
                throw;
            }
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            _logger?.Log("MqttService disconnect start", LogLevel.Debug);
            try
            {
                if (_subscriptions.Count > 0 && _client.IsConnected)
                {
                    var unsubscribeBuilder = new MqttClientUnsubscribeOptionsBuilder();
                    foreach (var t in _subscriptions)
                    {
                        unsubscribeBuilder.WithTopicFilter(t);
                    }
                    await _client.UnsubscribeAsync(unsubscribeBuilder.Build(), cancellationToken).ConfigureAwait(false);
                    _subscriptions.Clear();
                }

                if (_client.IsConnected)
                {
                    await _client.DisconnectAsync(new MqttClientDisconnectOptions(), cancellationToken).ConfigureAwait(false);
                }

                _logger?.Log("MqttService disconnect finished", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _logger?.Log($"MqttService disconnect error: {ex.Message}", LogLevel.Error);
                throw;
            }
        }

        public async Task SubscribeAsync(IEnumerable<string> topics, CancellationToken cancellationToken = default)
        {
            foreach (var t in topics)
            {
                _logger?.Log($"Subscribing to {t}", LogLevel.Debug);
                var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter(f => f.WithTopic(t))
                    .Build();
                await _client.SubscribeAsync(subscribeOptions, cancellationToken).ConfigureAwait(false);
                _subscriptions.Add(t);
            }
        }

        public async Task PublishAsync(string topic, string message, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtMostOnce, bool retainFlag = false, CancellationToken cancellationToken = default)
        {
            _logger?.Log("MqttService publish start", LogLevel.Debug);
            try
            {
                var msg = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(message)
                    .WithQualityOfServiceLevel(qos)
                    .WithRetainFlag(retainFlag)
                    .Build();
                await _client.PublishAsync(msg, cancellationToken).ConfigureAwait(false);
                _logger?.Log("MqttService publish finished", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _logger?.Log($"MqttService publish error: {ex.Message}", LogLevel.Error);
                throw;
            }
        }
    }
}
