using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.Services
{
    public class MqttService
    {
        private readonly IMqttClient _client;
        private readonly IMessageRoutingService _routingService;
        private readonly ILoggingService? _logger;

        public MqttService(IMessageRoutingService routingService, ILoggingService? logger = null)
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();
            _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
            _logger = logger;
        }

        internal MqttService(IMqttClient client, IMessageRoutingService routingService, ILoggingService? logger = null)
        {
            _client = client;
            _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
            _logger = logger;
        }

        public async Task ConnectAsync(string host, int port, string clientId, string? user, string? pass)
        {
            _logger?.Log("MqttService connect start", LogLevel.Debug);
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(host, port)
                .WithClientId(clientId);

            if (!string.IsNullOrEmpty(user))
                options = options.WithCredentials(user, pass);

            _logger?.Log($"Connecting to MQTT {host}:{port}", LogLevel.Debug);
            await _client.ConnectAsync(options.Build());
            _logger?.Log("MQTT connected", LogLevel.Debug);
            _logger?.Log("MqttService connect finished", LogLevel.Debug);
        }

        public async Task SubscribeAsync(IEnumerable<string> topics)
        {
            foreach (var t in topics)
            {
                _logger?.Log($"Subscribing to {t}", LogLevel.Debug);
                await _client.SubscribeAsync(t);
            }
        }

        public async Task PublishAsync(string topic, string message)
        {
            _logger?.Log("MqttService publish start", LogLevel.Debug);
            _logger?.Log($"Publishing to {topic}", LogLevel.Debug);
            var resolved = _routingService.ResolveTokens(message);
            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(resolved)
                .Build();
            await _client.PublishAsync(msg);
            _logger?.Log("Publish complete", LogLevel.Debug);
            _logger?.Log("MqttService publish finished", LogLevel.Debug);
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
    }
}
