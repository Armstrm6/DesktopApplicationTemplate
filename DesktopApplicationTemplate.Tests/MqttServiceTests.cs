using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Models;
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

            var options = new MqttServiceOptions { Host = "host", Port = 1234, ClientId = "id" };
            var service = new MqttService(client.Object, options);
            await service.ConnectAsync();
            await service.SubscribeAsync(new[] { "topic" });
            await service.PublishAsync("topic", "msg", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, true);

            client.Verify(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            client.Verify(c => c.SubscribeAsync(It.Is<MqttClientSubscribeOptions>(o => o.TopicFilters.Any(f => f.Topic == "topic")), It.IsAny<CancellationToken>()), Times.Once);
            client.Verify(c => c.PublishAsync(It.Is<MQTTnet.MqttApplicationMessage>(m => m.QualityOfServiceLevel == MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce && m.Retain == true), It.IsAny<CancellationToken>()), Times.Once);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public async Task ConnectAsync_DisconnectsExistingConnection()
        {
            var client = new Mock<IMqttClient>();
            client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MqttClientConnectResult());
            client.Setup(c => c.DisconnectAsync(It.IsAny<MqttClientDisconnectOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = new MqttService(client.Object);
            var options = new MqttServiceOptions { Host = "host", Port = 1, ClientId = "id" };
            await service.ConnectAsync(options);
            await service.ConnectAsync(options);

            client.Verify(c => c.DisconnectAsync(It.IsAny<MqttClientDisconnectOptions>(), It.IsAny<CancellationToken>()), Times.Once);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public async Task DisconnectAsync_UnsubscribesTopics()
        {
            var client = new Mock<IMqttClient>();
            client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MqttClientConnectResult());
            client.Setup(c => c.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MqttClientSubscribeResult(0, Array.Empty<MqttClientSubscribeResultItem>(), null!, Array.Empty<MqttUserProperty>()));
            client.Setup(c => c.UnsubscribeAsync(It.IsAny<MqttClientUnsubscribeOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MqttClientUnsubscribeResult(0, Array.Empty<MqttClientUnsubscribeResultItem>(), string.Empty, Array.Empty<MqttUserProperty>()));
            client.Setup(c => c.DisconnectAsync(It.IsAny<MqttClientDisconnectOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            client.SetupGet(c => c.IsConnected).Returns(true);

            var service = new MqttService(client.Object);
            var options = new MqttServiceOptions { Host = "host", Port = 1, ClientId = "id" };
            await service.ConnectAsync(options);
            await service.SubscribeAsync(new[] { "t1" });
            await service.DisconnectAsync();

            client.Verify(c => c.UnsubscribeAsync(It.Is<MqttClientUnsubscribeOptions>(o => o.TopicFilters.Contains("t1")), It.IsAny<CancellationToken>()), Times.Once);
            client.Verify(c => c.DisconnectAsync(It.IsAny<MqttClientDisconnectOptions>(), It.IsAny<CancellationToken>()), Times.Once);

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
