using MQTTnet;
using MQTTnet.Client;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.Services
{
    public class MqttService
    {
        private readonly IMqttClient _client;
        private readonly ILoggingService? _logger;

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

        public virtual async Task ConnectAsync(string host, int port, string clientId, string? user, string? pass, bool useTls = false, CancellationToken token = default)
        {
            _logger?.Log("MqttService connect start", LogLevel.Debug);
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
            _logger?.Log("MQTT connected", LogLevel.Debug);
            _logger?.Log("MqttService connect finished", LogLevel.Debug);
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
            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .Build();
            await _client.PublishAsync(msg).ConfigureAwait(false);
            _logger?.Log("Publish complete", LogLevel.Debug);
            _logger?.Log("MqttService publish finished", LogLevel.Debug);
        }

        public virtual Task DisconnectAsync(CancellationToken token = default)
        {
            return _client.DisconnectAsync(cancellationToken: token);
        }
    }
}
