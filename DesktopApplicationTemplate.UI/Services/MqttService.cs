using MQTTnet;
using MQTTnet.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .Build();
            await _client.PublishAsync(msg);
            _logger?.Log("Publish complete", LogLevel.Debug);
            _logger?.Log("MqttService publish finished", LogLevel.Debug);
        }
    }
}
