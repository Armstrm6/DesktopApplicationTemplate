using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Models;
using Moq;
using MQTTnet.Client;
using System.Threading.Tasks;
using System.Threading;
using MQTTnet;
using MQTTnet.Packets;
using System.Linq;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class MqttServiceTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public async Task ConnectAndPublish_CallsClient()
        {
            var client = new Mock<IMqttClient>();
            client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MqttClientConnectResult());
            client.Setup(c => c.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MqttClientSubscribeResult(0, Array.Empty<MqttClientSubscribeResultItem>(), null!, Array.Empty<MqttUserProperty>()));
            client.Setup(c => c.PublishAsync(It.IsAny<MQTTnet.MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MqttClientPublishResult(null, MqttClientPublishReasonCode.Success, null!, Array.Empty<MqttUserProperty>()));

            var options = new MqttServiceOptions { Host = "host", Port = 1234, ClientId = "id" };
            var service = new MqttService(client.Object, options);
            await service.ConnectAsync();
            await service.SubscribeAsync(new[] { "topic" });
            await service.PublishAsync("topic", "msg");

            client.Verify(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            client.Verify(c => c.SubscribeAsync(It.Is<MqttClientSubscribeOptions>(o => o.TopicFilters.Any(f => f.Topic == "topic")), It.IsAny<CancellationToken>()), Times.Once);
            client.Verify(c => c.PublishAsync(It.IsAny<MQTTnet.MqttApplicationMessage>(), It.IsAny<CancellationToken>()), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
