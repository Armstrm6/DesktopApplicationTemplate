using DesktopApplicationTemplate.UI.Services;
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

            var service = new MqttService(client.Object);
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
        [TestCategory("WindowsSafe")]
        public async Task ConnectAsync_DisconnectsBeforeReconnect()
        {
            var client = new Mock<IMqttClient>();
            client.SetupSequence(c => c.IsConnected).Returns(false).Returns(true);
            client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MqttClientConnectResult());
            client.Setup(c => c.DisconnectAsync(It.IsAny<MqttClientDisconnectOptions?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = new MqttService(client.Object);

            await service.ConnectAsync("host", 1883, "id", null, null);
            await service.ConnectAsync("host", 1883, "id", null, null);

            client.Verify(c => c.DisconnectAsync(It.IsAny<MqttClientDisconnectOptions?>(), It.IsAny<CancellationToken>()), Times.Once);
            client.Verify(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public async Task PublishAsync_SendsMessagesToMultipleEndpoints()
        {
            var client = new Mock<IMqttClient>();
            client.Setup(c => c.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MqttClientPublishResult(null, MqttClientPublishReasonCode.Success, null!, Array.Empty<MqttUserProperty>()));

            var service = new MqttService(client.Object);

            await service.PublishAsync("topic1", "static message");
            await service.PublishAsync("topic2", "{ServiceName.Message}");

            client.Verify(c => c.PublishAsync(It.Is<MqttApplicationMessage>(m => m.Topic == "topic1"), It.IsAny<CancellationToken>()), Times.Once);
            client.Verify(c => c.PublishAsync(It.Is<MqttApplicationMessage>(m => m.Topic == "topic2"), It.IsAny<CancellationToken>()), Times.Once);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public async Task DisconnectAsync_CallsClientWhenConnected()
        {
            var client = new Mock<IMqttClient>();
            client.Setup(c => c.IsConnected).Returns(true);
            client.Setup(c => c.DisconnectAsync(It.IsAny<MqttClientDisconnectOptions?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = new MqttService(client.Object);
            await service.DisconnectAsync();

            client.Verify(c => c.DisconnectAsync(It.IsAny<MqttClientDisconnectOptions?>(), It.IsAny<CancellationToken>()), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
