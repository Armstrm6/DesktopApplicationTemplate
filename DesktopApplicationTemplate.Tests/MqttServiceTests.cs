using System;
using System.Collections.Generic;
using System.Linq;
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
using MQTTnet.Protocol;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class MqttServiceTests
{
    [Fact]
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
    public async Task ConnectAsync_AppliesWillAndKeepAliveOptions()
    {
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult());
        var options = Options.Create(new MqttServiceOptions
        {
            Host = "h",
            Port = 1,
            ClientId = "id",
            WillTopic = "t",
            WillPayload = "p",
            WillQualityOfService = MqttQualityOfServiceLevel.AtLeastOnce,
            WillRetain = true,
            KeepAliveSeconds = 10,
            CleanSession = false
        });
        var service = new MqttService(client.Object, options, Mock.Of<IMessageRoutingService>(), Mock.Of<ILoggingService>());

        await service.ConnectAsync();

        client.Verify(c => c.ConnectAsync(It.Is<MqttClientOptions>(o =>
            o.WillTopic == options.Value.WillTopic &&
            o.WillPayload.SequenceEqual(System.Text.Encoding.UTF8.GetBytes(options.Value.WillPayload!)) &&
            o.WillQualityOfServiceLevel == options.Value.WillQualityOfService &&
            o.WillRetain == options.Value.WillRetain &&
            o.KeepAlivePeriod == TimeSpan.FromSeconds(options.Value.KeepAliveSeconds) &&
            o.CleanSession == options.Value.CleanSession
        ), It.IsAny<CancellationToken>()), Times.Once);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    public async Task ConnectAsync_Retries_When_Failure_And_ReconnectDelay_Set()
    {
        var client = new Mock<IMqttClient>();
        client.SetupSequence(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MQTTnet.Exceptions.MqttCommunicationException("fail"))
            .ReturnsAsync(new MqttClientConnectResult());
        var options = Options.Create(new MqttServiceOptions { Host = "h", Port = 1, ClientId = "id", ReconnectDelay = TimeSpan.FromMilliseconds(1) });
        var service = new MqttService(client.Object, options, Mock.Of<IMessageRoutingService>(), Mock.Of<ILoggingService>());

        await service.ConnectAsync();

        client.Verify(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    public async Task ConnectAsync_AppliesTlsAndCredentials()
    {
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult());
        var cert = new byte[] { 1, 2, 3 };
        var options = Options.Create(new MqttServiceOptions
        {
            Host = "h",
            Port = 1,
            ClientId = "id",
            Username = "u",
            Password = "p",
            ConnectionType = MqttConnectionType.MqttTls,
            ClientCertificate = cert
        });
        var service = new MqttService(client.Object, options, Mock.Of<IMessageRoutingService>(), Mock.Of<ILoggingService>());

        await service.ConnectAsync();

        client.Verify(c => c.ConnectAsync(It.Is<MqttClientOptions>(o =>
            o.Credentials != null &&
            o.Credentials.GetUserName(o) == "u" &&
            Encoding.UTF8.GetString(o.Credentials.GetPassword(o)) == "p" &&
            o.ChannelOptions != null &&
            o.ChannelOptions.TlsOptions != null
        ), It.IsAny<CancellationToken>()), Times.Once);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    public async Task SubscribeAsync_ForwardsQoS()
    {
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientSubscribeResult(0, Array.Empty<MqttClientSubscribeResultItem>(), string.Empty, Array.Empty<MqttUserProperty>()));
        var service = new MqttService(client.Object, Options.Create(new MqttServiceOptions()), Mock.Of<IMessageRoutingService>(), Mock.Of<ILoggingService>());

        await service.SubscribeAsync("t", MqttQualityOfServiceLevel.ExactlyOnce);

        client.Verify(c => c.SubscribeAsync(It.Is<MqttClientSubscribeOptions>(o =>
            o.TopicFilters.Any(f => f.Topic == "t" && f.QualityOfServiceLevel == MqttQualityOfServiceLevel.ExactlyOnce)),
            It.IsAny<CancellationToken>()), Times.Once);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
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
