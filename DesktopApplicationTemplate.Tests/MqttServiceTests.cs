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
        public async Task ConnectAsync_TranslatesOptions_AndHandlesTls()
        {
            var client = new Mock<IMqttClient>();
            MqttClientOptions? captured = null;
            client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
                .Callback<MqttClientOptions, CancellationToken>((o, _) => captured = o)
                .ReturnsAsync(new MqttClientConnectResult());

            var service = new MqttService(client.Object);

            await service.ConnectAsync("server", 8883, "cid", "u", "p", true, CancellationToken.None);

            Assert.NotNull(captured);
            var tcp = (MqttClientTcpOptions)captured!.ChannelOptions!;
            var endpoint = (System.Net.DnsEndPoint)tcp.RemoteEndpoint!;
            Assert.Equal("server", endpoint.Host);
            Assert.Equal(8883, endpoint.Port);
            Assert.Equal("cid", captured.ClientId);
            Assert.Equal("u", captured.Credentials?.GetUserName(captured));
            Assert.Equal("p", System.Text.Encoding.UTF8.GetString(captured.Credentials!.GetPassword(captured)!));
            Assert.NotNull(tcp.TlsOptions);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public async Task ConnectAsync_DisconnectsBeforeReconnecting()
        {
            var client = new Mock<IMqttClient>();
            var isConnected = false;
            client.SetupGet(c => c.IsConnected).Returns(() => isConnected);
            client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MqttClientConnectResult())
                .Callback(() => isConnected = true);
            client.Setup(c => c.DisconnectAsync(It.IsAny<MqttClientDisconnectOptions?>(), It.IsAny<CancellationToken>()))
                .Returns(() => { isConnected = false; return Task.CompletedTask; });

            var service = new MqttService(client.Object);

            await service.ConnectAsync("h1", 1234, "id1", null, null);
            await service.ConnectAsync("h2", 1234, "id2", null, null);

            client.Verify(c => c.DisconnectAsync(It.IsAny<MqttClientDisconnectOptions?>(), It.IsAny<CancellationToken>()), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
