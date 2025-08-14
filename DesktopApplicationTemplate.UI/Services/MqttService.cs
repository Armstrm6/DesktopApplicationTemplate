using MQTTnet;
using MQTTnet.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using Microsoft.Extensions.Options;

namespace DesktopApplicationTemplate.UI.Services
{
    public class MqttService
    {
        private readonly IMqttClient _client;
        private readonly ILoggingService _logger;
        private readonly MqttServiceOptions _options;

        public MqttService(IOptions<MqttServiceOptions> options, ILoggingService logger)
        {
            _options = options.Value;
            _logger = logger;
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();
        }

        internal MqttService(IMqttClient client, IOptions<MqttServiceOptions> options, ILoggingService logger)
        {
            _client = client;
            _logger = logger;
            _options = options.Value;
        }

        public async Task ConnectAsync(string? host = null, int? port = null, string? clientId = null, string? user = null, string? pass = null)
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
        }

        public async Task SubscribeAsync(IEnumerable<string> topics)
        {
            foreach (var t in topics)
            {
                _logger.Log($"Subscribing to {t}", LogLevel.Debug);
                await _client.SubscribeAsync(t).ConfigureAwait(false);
            }
        }

        public async Task PublishAsync(string topic, string message)
        {
            _logger.Log("MqttService publish start", LogLevel.Debug);
            _logger.Log($"Publishing to {topic}", LogLevel.Debug);
            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .Build();
            await _client.PublishAsync(msg).ConfigureAwait(false);
            _logger.Log("Publish complete", LogLevel.Debug);
            _logger.Log("MqttService publish finished", LogLevel.Debug);
        }
    }
}
