using MQTTnet;
using MQTTnet.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services
{
    public class MqttService
    {
        private readonly IMqttClient _client;

        public MqttService()
        {
            // MqttFactory is provided by MQTTnet and is used to create clients
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();
        }

        public async Task ConnectAsync(string host, int port, string clientId, string? user, string? pass)
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(host, port)
                .WithClientId(clientId);

            if (!string.IsNullOrEmpty(user))
                options = options.WithCredentials(user, pass);

            await _client.ConnectAsync(options.Build());
        }

        public async Task SubscribeAsync(IEnumerable<string> topics)
        {
            foreach (var t in topics)
            {
                await _client.SubscribeAsync(t);
            }
        }

        public async Task PublishAsync(string topic, string message)
        {
            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .Build();
            await _client.PublishAsync(msg);
        }
    }
}
