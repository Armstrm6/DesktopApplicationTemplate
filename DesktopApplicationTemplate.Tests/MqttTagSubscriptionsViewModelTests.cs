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

    [SkippableFact]
    public async Task ConnectAsync_InvokesClient()
    {
        Skip.IfNot(OperatingSystem.IsWindows(), "Requires Windows desktop runtime");
        var (vm, client, _) = CreateViewModel();
        await vm.ConnectAsync();
        client.Verify(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [SkippableFact]
    public async Task ConnectAsync_RaisesEditRequested_OnInvalidOptions()
    {
        Skip.IfNot(OperatingSystem.IsWindows(), "Requires Windows desktop runtime");
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Host cannot be null or whitespace."));
        var options = Options.Create(new MqttServiceOptions());
        var service = new MqttService(client.Object, options, Mock.Of<IMessageRoutingService>(), Mock.Of<ILoggingService>());
        var vm = new MqttTagSubscriptionsViewModel(service);
        bool raised = false;
        vm.EditConnectionRequested += (_, _) => raised = true;
        await vm.ConnectAsync();
        Assert.True(raised);
    }

    [SkippableFact]
    public async Task AddTopicAsync_SubscribesAndAdds()
    {
        Skip.IfNot(OperatingSystem.IsWindows(), "Requires Windows desktop runtime");
        var (vm, client, _) = CreateViewModel();
        vm.NewTopic = "t";
        vm.NewQoS = MqttQualityOfServiceLevel.AtLeastOnce;
        await ((AsyncRelayCommand)vm.AddTopicCommand).ExecuteAsync();

        client.Verify(c => c.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Contains(vm.Subscriptions, s => s.Topic == "t" && s.QoS == MqttQualityOfServiceLevel.AtLeastOnce);
        Assert.Equal(string.Empty, vm.NewTopic);
        Assert.Contains(vm.SubscriptionResults, r => r.Topic == "t" && r.IsSuccess);
    }

    [SkippableFact]
    public async Task RemoveTopicAsync_UnsubscribesAndRemoves()
    {
        Skip.IfNot(OperatingSystem.IsWindows(), "Requires Windows desktop runtime");
        var (vm, client, service) = CreateViewModel();
        var sub = new TagSubscription("t");
        service.UpdateTagSubscription(sub);
        vm.Subscriptions.Add(sub);
        vm.SelectedSubscription = sub;
        await ((AsyncRelayCommand)vm.RemoveTopicCommand).ExecuteAsync();
        client.Verify(c => c.UnsubscribeAsync(It.IsAny<MqttClientUnsubscribeOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Empty(vm.Subscriptions);
        Assert.Null(vm.SelectedSubscription);
    }

    [SkippableFact]
    public async Task PublishTestMessageAsync_Publishes_WhenValid()
    {
        Skip.IfNot(OperatingSystem.IsWindows(), "Requires Windows desktop runtime");
        var (vm, client, _) = CreateViewModel();
        var sub = new TagSubscription("t") { OutgoingMessage = "m" };
        vm.Subscriptions.Add(sub);
        vm.SelectedSubscription = sub;
        await ((AsyncRelayCommand)vm.PublishTestMessageCommand).ExecuteAsync();
        client.Verify(c => c.PublishAsync(It.Is<MQTTnet.MqttApplicationMessage>(m => m.Topic == "t"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [SkippableFact]
    public async Task TestTagEndpointCommand_Publishes_WhenValid()
    {
        Skip.IfNot(OperatingSystem.IsWindows(), "Requires Windows desktop runtime");
        var (vm, client, _) = CreateViewModel();
        var sub = new TagSubscription("tag") { Endpoint = "e", OutgoingMessage = "m" };
        await ((AsyncRelayCommand<TagSubscription>)vm.TestTagEndpointCommand).ExecuteAsync(sub);
        client.Verify(c => c.PublishAsync(It.Is<MQTTnet.MqttApplicationMessage>(m => m.Topic == "e"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
