using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.Extensions.Options;
using Moq;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class MqttServiceTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public async Task ConnectAsync_RaisesConnectionStateChanged()
    {
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult());
        var options = Options.Create(new MqttServiceOptions { Host = "h", Port = 1, ClientId = "id" });
        var routing = new Mock<IMessageRoutingService>().Object;
        var logger = new Mock<ILoggingService>().Object;
        var service = new MqttService(client.Object, options, routing, logger);
        bool raised = false;
        service.ConnectionStateChanged += (_, c) => raised = c;

        await service.ConnectAsync();

        Assert.True(raised);
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

        var options = Options.Create(new MqttServiceOptions { Host = "h", Port = 1, ClientId = "id" });
        var service = new MqttService(client.Object, options, Mock.Of<IMessageRoutingService>(), Mock.Of<ILoggingService>());

        await service.ConnectAsync();
        await service.ConnectAsync();

        client.Verify(c => c.DisconnectAsync(It.IsAny<MqttClientDisconnectOptions?>(), It.IsAny<CancellationToken>()), Times.Once);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    [TestCategory("CodexSafe")]
    public async Task PublishAsync_ResolvesTokensBeforeSending()
    {
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientPublishResult(null, MqttClientPublishReasonCode.Success, null!, Array.Empty<MqttUserProperty>()));
        var routing = new Mock<IMessageRoutingService>();
        routing.Setup(r => r.ResolveTokens("input")).Returns("resolved");
        var service = new MqttService(client.Object, Options.Create(new MqttServiceOptions()), routing.Object, Mock.Of<ILoggingService>());

        await service.PublishAsync("topic", "input");

        routing.Verify(r => r.ResolveTokens("input"), Times.Once);
        client.Verify(c => c.PublishAsync(It.Is<MqttApplicationMessage>(m => Encoding.UTF8.GetString(m.PayloadSegment.Array!, m.PayloadSegment.Offset, m.PayloadSegment.Count) == "resolved"), It.IsAny<CancellationToken>()), Times.Once);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    [TestCategory("CodexSafe")]
    public async Task PublishAsync_SendsMessagesToMultipleEndpoints()
    {
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientPublishResult(null, MqttClientPublishReasonCode.Success, null!, Array.Empty<MqttUserProperty>()));
        var routing = new Mock<IMessageRoutingService>();
        routing.Setup(r => r.ResolveTokens(It.IsAny<string>())).Returns<string>(s => s);
        var service = new MqttService(client.Object, Options.Create(new MqttServiceOptions()), routing.Object, Mock.Of<ILoggingService>());

        var map = new Dictionary<string, IEnumerable<string>>
        {
            ["t1"] = new[] { "m1", "m2" },
            ["t2"] = new[] { "m3" }
        };

        await service.PublishAsync(map);

        client.Verify(c => c.PublishAsync(It.Is<MqttApplicationMessage>(m => m.Topic == "t1"), It.IsAny<CancellationToken>()), Times.Exactly(2));
        client.Verify(c => c.PublishAsync(It.Is<MqttApplicationMessage>(m => m.Topic == "t2"), It.IsAny<CancellationToken>()), Times.Once);
        ConsoleTestLogger.LogPass();
    }
}
