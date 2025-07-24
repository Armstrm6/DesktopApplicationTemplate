using DesktopApplicationTemplate.Core.Services;
using Moq;
using MQTTnet.Client;
using System.Threading.Tasks;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class MqttServiceTests
    {
        [Fact]
        public async Task ConnectAndPublish_CallsClient()
        {
            var client = new Mock<IMqttClient>();
            client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>())).Returns(Task.CompletedTask);
            client.Setup(c => c.SubscribeAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            client.Setup(c => c.PublishAsync(It.IsAny<MQTTnet.MqttApplicationMessage>())).Returns(Task.CompletedTask);

            var service = new MqttService(client.Object);
            await service.ConnectAsync("host", 1234, "id", null, null);
            await service.SubscribeAsync(new[] { "topic" });
            await service.PublishAsync("topic", "msg");

            client.Verify(c => c.ConnectAsync(It.IsAny<MqttClientOptions>()), Times.Once);
            client.Verify(c => c.SubscribeAsync("topic"), Times.Once);
            client.Verify(c => c.PublishAsync(It.IsAny<MQTTnet.MqttApplicationMessage>()), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
