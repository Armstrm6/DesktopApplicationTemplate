using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Moq;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet.Packets;
using Microsoft.Extensions.Options;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class MqttTagSubscriptionsViewModelTests
{
    private static MqttTagSubscriptionsViewModel CreateViewModel(Mock<IMqttClient>? clientMock = null)
    {
        var logger = Mock.Of<ILoggingService>();
        var options = Options.Create(new MqttServiceOptions { Host = "localhost", Port = 1883, ClientId = "client" });
        var routing = new Mock<IMessageRoutingService>();
        var client = clientMock ?? new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult());
        client.Setup(c => c.PublishAsync(It.IsAny<MQTTnet.MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientPublishResult(null, MqttClientPublishReasonCode.Success, null!, Array.Empty<MQTTnet.Packets.MqttUserProperty>()));
        client.Setup(c => c.SubscribeAsync(It.IsAny<string>(), It.IsAny<MqttQualityOfServiceLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientSubscribeResult());
        client.Setup(c => c.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientSubscribeResult(0, Array.Empty<MqttClientSubscribeResultItem>(), null!, Array.Empty<MQTTnet.Packets.MqttUserProperty>()));
        client.Setup(c => c.UnsubscribeAsync(It.IsAny<MqttClientUnsubscribeOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientUnsubscribeResult(0, Array.Empty<MqttClientUnsubscribeResultItem>(), null!, Array.Empty<MQTTnet.Packets.MqttUserProperty>()));
        var service = new MqttService(client.Object, options, routing.Object, logger);
        return new MqttTagSubscriptionsViewModel(service);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public async Task ConnectAsync_InvokesClient()
    {
        if (!OperatingSystem.IsWindows()) return;
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult());
        client.Setup(c => c.SubscribeAsync(It.IsAny<string>(), It.IsAny<MqttQualityOfServiceLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientSubscribeResult());
        var vm = CreateViewModel(client);
        await vm.ConnectAsync();
        client.Verify(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(vm.IsConnected);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public async Task ConnectAsync_SubscribesWithSelectedQoS()
    {
        if (!OperatingSystem.IsWindows()) return;
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult());
        client.Setup(c => c.SubscribeAsync("t", MqttQualityOfServiceLevel.AtLeastOnce, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientSubscribeResult());
        var vm = CreateViewModel(client);
        vm.Topics.Add(new TagSubscription { Topic = "t", QoS = MqttQualityOfServiceLevel.AtLeastOnce });
        await vm.ConnectAsync();
        client.Verify(c => c.SubscribeAsync("t", MqttQualityOfServiceLevel.AtLeastOnce, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public async Task PublishTestAsync_Publishes_WhenValid()
    {
        if (!OperatingSystem.IsWindows()) return;
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult());
        client.Setup(c => c.PublishAsync(It.IsAny<MQTTnet.MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientPublishResult(null, MqttClientPublishReasonCode.Success, null!, Array.Empty<MQTTnet.Packets.MqttUserProperty>()));
        var vm = CreateViewModel(client);
        var sub = new TagSubscription { Topic = "t", QoS = MqttQualityOfServiceLevel.AtMostOnce };
        vm.Topics.Add(sub);
        vm.SelectedTopic = sub;
        vm.TestMessage = "m";
        vm.Subscriptions.Add(new TagSubscription { Topic = "t", OutgoingMessage = "m" });
        vm.SelectedSubscription = vm.Subscriptions.First();
        await vm.PublishTestAsync();
        client.Verify(c => c.PublishAsync(It.Is<MQTTnet.MqttApplicationMessage>(m => m.Topic == "t"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public async Task PublishTestAsync_DoesNothing_WhenInvalid()
    {
        if (!OperatingSystem.IsWindows()) return;
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.PublishAsync(It.IsAny<MQTTnet.MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientPublishResult(null, MqttClientPublishReasonCode.Success, null!, Array.Empty<MQTTnet.Packets.MqttUserProperty>()));
        var vm = CreateViewModel(client);
        await vm.PublishTestAsync();
        client.Verify(c => c.PublishAsync(It.IsAny<MQTTnet.MqttApplicationMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public async Task AddTopic_AddsSubscriptionAndClearsInput()
    {
        if (!OperatingSystem.IsWindows()) return;
        var vm = CreateViewModel();
        vm.NewTopic = "topic";
        vm.AddTopicCommand.Execute(null);
        Assert.Contains(vm.Topics, t => t.Topic == "topic");
        await vm.AddTopicAsync();
        Assert.Contains(vm.Subscriptions, s => s.Topic == "topic");
        Assert.Equal(string.Empty, vm.NewTopic);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public async Task AddTopic_IgnoresEmptyInput()
    {
        if (!OperatingSystem.IsWindows()) return;
        var client = new Mock<IMqttClient>();
        var vm = CreateViewModel(client);
        vm.NewTopic = "   ";
        await vm.AddTopicAsync();
        client.Verify(c => c.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Empty(vm.Subscriptions);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public async Task RemoveTopic_RemovesSelectedSubscription()
    {
        if (!OperatingSystem.IsWindows()) return;
        var vm = CreateViewModel();
        var sub = new TagSubscription { Topic = "t", QoS = MqttQualityOfServiceLevel.AtMostOnce };
        vm.Topics.Add(sub);
        vm.SelectedTopic = sub;
        vm.RemoveTopicCommand.Execute(null);
        Assert.Empty(vm.Topics);
        Assert.Null(vm.SelectedTopic);
        vm.Subscriptions.Add(new TagSubscription { Topic = "t" });
        vm.SelectedSubscription = vm.Subscriptions.First();
        await vm.RemoveTopicAsync();
        Assert.Empty(vm.Subscriptions);
        Assert.Null(vm.SelectedSubscription);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public async Task AddTopicAsync_RecordsSuccessResult()
    {
        if (!OperatingSystem.IsWindows()) return;
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientSubscribeResult(0, Array.Empty<MqttClientSubscribeResultItem>(), null!, Array.Empty<MqttUserProperty>()));
        var vm = CreateViewModel(client);
        vm.NewTopic = "a";
        await vm.AddTopicAsync();
        client.Verify(c => c.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Contains(vm.SubscriptionResults, r => r.Topic == "a" && r.IsSuccess);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public async Task AddTopicAsync_RecordsFailureResult()
    {
        if (!OperatingSystem.IsWindows()) return;
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("fail"));
        var vm = CreateViewModel(client);
        vm.NewTopic = "a";
        await vm.AddTopicAsync();
        Assert.Contains(vm.SubscriptionResults, r => r.Topic == "a" && !r.IsSuccess);
        Assert.Empty(vm.Subscriptions);
    }
}
