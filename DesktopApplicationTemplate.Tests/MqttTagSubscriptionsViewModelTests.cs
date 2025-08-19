using System;
using System.Linq;
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
using MQTTnet.Packets;
using Microsoft.Extensions.Options;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class MqttTagSubscriptionsViewModelTests
{
    private static (MqttTagSubscriptionsViewModel vm, Mock<IMqttClient> client, MqttService service) CreateViewModel()
    {
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult());
        client.Setup(c => c.PublishAsync(It.IsAny<MQTTnet.MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientPublishResult(null, MqttClientPublishReasonCode.Success, null!, Array.Empty<MqttUserProperty>()));
        client.Setup(c => c.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientSubscribeResult(0, Array.Empty<MqttClientSubscribeResultItem>(), null!, Array.Empty<MqttUserProperty>()));
        client.Setup(c => c.UnsubscribeAsync(It.IsAny<MqttClientUnsubscribeOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientUnsubscribeResult(0, Array.Empty<MqttClientUnsubscribeResultItem>(), null!, Array.Empty<MqttUserProperty>()));

        var logger = Mock.Of<ILoggingService>();
        var options = Options.Create(new MqttServiceOptions { Host = "localhost", Port = 1883, ClientId = "client" });
        var routing = Mock.Of<IMessageRoutingService>();
        var service = new MqttService(client.Object, options, routing, logger);
        var vm = new MqttTagSubscriptionsViewModel(service);
        return (vm, client, service);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public async Task ConnectAsync_InvokesClient()
    {
        if (!OperatingSystem.IsWindows()) return;
        var (vm, client, _) = CreateViewModel();
        await vm.ConnectAsync();
        client.Verify(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public async Task AddTopicAsync_SubscribesAndAdds()
    {
        if (!OperatingSystem.IsWindows()) return;
        var (vm, client, _) = CreateViewModel();
        vm.NewTopic = "t";
        vm.NewQoS = MqttQualityOfServiceLevel.AtLeastOnce;
        await ((AsyncRelayCommand)vm.AddTopicCommand).ExecuteAsync(null);

        client.Verify(c => c.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Contains(vm.Subscriptions, s => s.Topic == "t" && s.QoS == MqttQualityOfServiceLevel.AtLeastOnce);
        Assert.Equal(string.Empty, vm.NewTopic);
        Assert.Contains(vm.SubscriptionResults, r => r.Topic == "t" && r.IsSuccess);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public async Task RemoveTopicAsync_UnsubscribesAndRemoves()
    {
        if (!OperatingSystem.IsWindows()) return;
        var (vm, client, service) = CreateViewModel();
        var sub = new TagSubscription("t");
        service.UpdateTagSubscription(sub);
        vm.Subscriptions.Add(sub);
        vm.SelectedSubscription = sub;
        await ((AsyncRelayCommand)vm.RemoveTopicCommand).ExecuteAsync(null);
        client.Verify(c => c.UnsubscribeAsync(It.IsAny<MqttClientUnsubscribeOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Empty(vm.Subscriptions);
        Assert.Null(vm.SelectedSubscription);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public async Task PublishTestMessageAsync_Publishes_WhenValid()
    {
        if (!OperatingSystem.IsWindows()) return;
        var (vm, client, _) = CreateViewModel();
        var sub = new TagSubscription("t") { OutgoingMessage = "m" };
        vm.Subscriptions.Add(sub);
        vm.SelectedSubscription = sub;
        await ((AsyncRelayCommand)vm.PublishTestMessageCommand).ExecuteAsync(null);
        client.Verify(c => c.PublishAsync(It.Is<MQTTnet.MqttApplicationMessage>(m => m.Topic == "t"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public async Task TestTagEndpointCommand_Publishes_WhenValid()
    {
        if (!OperatingSystem.IsWindows()) return;
        var (vm, client, _) = CreateViewModel();
        var sub = new TagSubscription("tag") { Endpoint = "e", OutgoingMessage = "m" };
        await ((AsyncRelayCommand<TagSubscription>)vm.TestTagEndpointCommand).ExecuteAsync(sub);
        client.Verify(c => c.PublishAsync(It.Is<MQTTnet.MqttApplicationMessage>(m => m.Topic == "e"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
