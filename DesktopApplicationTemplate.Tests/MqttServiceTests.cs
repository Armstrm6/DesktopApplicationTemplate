using DesktopApplicationTemplate.UI.Services;
using Moq;
using MQTTnet.Client;
using System.Threading.Tasks;
using System.Threading;
using MQTTnet;
using MQTTnet.Packets;
using System.Linq;
using System.Text;
using System.Collections.Generic;
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

            var routing = new MessageRoutingService();
            var service = new MqttService(client.Object, routing);
            await service.ConnectAsync("host", 1234, "id", null, null);
            await service.SubscribeAsync(new[] { "topic" });
            await service.PublishAsync("topic", "msg");

            client.Verify(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            client.Verify(c => c.SubscribeAsync(It.Is<MqttClientSubscribeOptions>(o => o.TopicFilters.Any(f => f.Topic == "topic")), It.IsAny<CancellationToken>()), Times.Once);
            client.Verify(c => c.PublishAsync(It.IsAny<MQTTnet.MqttApplicationMessage>(), It.IsAny<CancellationToken>()), Times.Once);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        public async Task PublishAsync_ResolvesTokensAndPublishesAllMessages()
        {
            var client = new Mock<IMqttClient>();
            client.Setup(c => c.PublishAsync(It.IsAny<MQTTnet.MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MqttClientPublishResult(null, MqttClientPublishReasonCode.Success, null!, Array.Empty<MqttUserProperty>()));

            var routing = new MessageRoutingService();
            routing.UpdateMessage("svc1", "one");
            routing.UpdateMessage("svc2", "two");

            var service = new MqttService(client.Object, routing);
            var map = new Dictionary<string, IEnumerable<string>>
            {
                ["topic"] = new[] { "{svc1.Message}", "{svc2.Message}" }
            };

            await service.PublishAsync(map);

            client.Verify(c => c.PublishAsync(It.Is<MQTTnet.MqttApplicationMessage>(m => Encoding.UTF8.GetString(m.PayloadSegment) == "one"), It.IsAny<CancellationToken>()), Times.Once);
            client.Verify(c => c.PublishAsync(It.Is<MQTTnet.MqttApplicationMessage>(m => Encoding.UTF8.GetString(m.PayloadSegment) == "two"), It.IsAny<CancellationToken>()), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
