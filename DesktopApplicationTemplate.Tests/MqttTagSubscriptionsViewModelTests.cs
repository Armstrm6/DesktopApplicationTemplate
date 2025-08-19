using System;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Moq;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Microsoft.Extensions.Options;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class MqttTagSubscriptionsViewModelTests
{
    private static (MqttTagSubscriptionsViewModel vm, Mock<IMqttClient> clientMock) CreateViewModel()
    {
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(new MqttClientConnectResult());
        client.Setup(c => c.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(new MqttClientSubscribeResult(0, Array.Empty<MqttClientSubscribeResultItem>(), null!, Array.Empty<MQTTnet.Packets.MqttUserProperty>()));
        client.Setup(c => c.PublishAsync(It.IsAny<MQTTnet.MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(new MqttClientPublishResult(null, MqttClientPublishReasonCode.Success, null!, Array.Empty<MQTTnet.Packets.MqttUserProperty>()));

        var options = Options.Create(new MqttServiceOptions { Host = "h", Port = 1, ClientId = "c" });
        var routing = Mock.Of<IMessageRoutingService>();
        var logger = Mock.Of<ILoggingService>();
        var service = new MqttService(client.Object, options, routing, logger);
        var vm = new MqttTagSubscriptionsViewModel(service);
        return (vm, client);
    }

    [Fact]
    public async Task ConnectAsync_SubscribesToTopics()
    {
        if (!OperatingSystem.IsWindows()) return;
        var (vm, client) = CreateViewModel();
        vm.Subscriptions.Add(new TagSubscription { Tag = "t", Topic = "topic", QoS = MqttQualityOfServiceLevel.ExactlyOnce });
        await vm.ConnectAsync();
        client.Verify(c => c.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void AddTopic_AddsSubscriptionAndClearsInputs()
    {
        if (!OperatingSystem.IsWindows()) return;
        var (vm, _) = CreateViewModel();
        vm.NewTag = "tag";
        vm.NewTopic = "topic";
        vm.AddTopicCommand.Execute(null);
        Assert.Single(vm.Subscriptions);
        Assert.Equal(string.Empty, vm.NewTag);
        Assert.Equal(string.Empty, vm.NewTopic);
    }

    [Fact]
    public async Task TestTagEndpointCommand_Publishes_WhenValid()
    {
        if (!OperatingSystem.IsWindows()) return;
        var (vm, client) = CreateViewModel();
        var sub = new TagSubscription { Tag = "t", Topic = "topic", Endpoint = "e", OutgoingMessage = "m" };
        vm.Subscriptions.Add(sub);
        await vm.TestTagEndpointCommand.ExecuteAsync(sub);
        client.Verify(c => c.PublishAsync(It.IsAny<MQTTnet.MqttApplicationMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
