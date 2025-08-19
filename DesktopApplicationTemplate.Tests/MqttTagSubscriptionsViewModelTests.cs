using System.Collections.Generic;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
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
    private static (MqttTagSubscriptionsViewModel vm, MqttService service) CreateViewModel(Mock<IMqttClient>? clientMock = null, IEnumerable<TagSubscription>? tags = null)
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
        if (tags != null)
        {
            foreach (var tag in tags)
            {
                service.UpdateTagSubscription(tag);
            }
        }
        var vm = new MqttTagSubscriptionsViewModel(service);
        return (vm, service);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public async Task ConnectAsync_InvokesClient()
    {
        if (!OperatingSystem.IsWindows()) return;
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult());
        var (vm, _) = CreateViewModel(client);
        client.Setup(c => c.SubscribeAsync(It.IsAny<string>(), It.IsAny<MqttQualityOfServiceLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientSubscribeResult());
        await vm.ConnectAsync();
        client.Verify(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public async Task TestTagEndpointCommand_Publishes_WhenValid()
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
        var publishCalled = false;
        client.Setup(c => c.PublishAsync(It.Is<MQTTnet.MqttApplicationMessage>(m => m.Topic == "t"), It.IsAny<CancellationToken>()))
            .Callback(() => publishCalled = true)
            .ReturnsAsync(new MqttClientPublishResult(null, MqttClientPublishReasonCode.Success, null!, Array.Empty<MQTTnet.Packets.MqttUserProperty>()));
        var (vm, _) = CreateViewModel(client);
        var sub = new TagSubscription("t");
        vm.Subscriptions.Add(sub);
        vm.SelectedSubscription = sub;
        var vm = CreateViewModel(client);
        var sub = new TagSubscription { Tag = "tag", Endpoint = "t", OutgoingMessage = "m" };
        vm.Subscriptions.Add(sub);
        await ((AsyncRelayCommand<TagSubscription>)vm.TestTagEndpointCommand).ExecuteAsync(sub);
        Assert.True(publishCalled);
        var sub = new TagSubscription { Tag = "t", OutgoingMessage = "m" };
        vm.TagSubscriptions.Add(sub);
        vm.SelectedSubscription = sub;
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
    public void TestTagEndpointCommand_CanExecute_ReturnsFalse_WhenEndpointOrMessageMissing()
    {
        if (!OperatingSystem.IsWindows()) return;
        var vm = CreateViewModel();
        var sub = new TagSubscription { Tag = "t" };
        vm.Subscriptions.Add(sub);
        var cmd = vm.TestTagEndpointCommand;
        Assert.False(cmd.CanExecute(sub));
        sub.Endpoint = "e";
        Assert.False(cmd.CanExecute(sub));
        sub.Endpoint = string.Empty;
        sub.OutgoingMessage = "m";
        Assert.False(cmd.CanExecute(sub));
        sub.Endpoint = "e";
        Assert.True(cmd.CanExecute(sub));
        sub.OutgoingMessage = string.Empty;
        Assert.False(cmd.CanExecute(sub));
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.PublishAsync(It.IsAny<MQTTnet.MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientPublishResult(null, MqttClientPublishReasonCode.Success, null!, Array.Empty<MQTTnet.Packets.MqttUserProperty>()));
        var (vm, _) = CreateViewModel(client);
        await vm.PublishTestAsync();
        client.Verify(c => c.PublishAsync(It.IsAny<MQTTnet.MqttApplicationMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public void AddTag_AddsTagAndClearsInput()
    {
        if (!OperatingSystem.IsWindows()) return;
        var vm = CreateViewModel();
        vm.NewTag = "tag";
        vm.AddTagCommand.Execute(null);
        Assert.Contains(vm.Subscriptions, s => s.Tag == "tag");
        Assert.Equal(string.Empty, vm.NewTag);
    public async Task AddTopic_AddsSubscriptionAndClearsInput()
    {
        if (!OperatingSystem.IsWindows()) return;
        var (vm, _) = CreateViewModel();
        vm.NewTopic = "topic";
        vm.AddTopicCommand.Execute(null);

        Assert.Contains(vm.TagSubscriptions, t => t.Tag == "topic");
        Assert.Contains(vm.Topics, t => t.Topic == "topic");
        await vm.AddTopicAsync();
        Assert.Contains(vm.Subscriptions, s => s.Topic == "topic");
        Assert.Equal(string.Empty, vm.NewTopic);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public void AddTag_IgnoresEmptyInput()
    {
        if (!OperatingSystem.IsWindows()) return;
        var vm = CreateViewModel();
        vm.NewTag = "   ";
        vm.AddTagCommand.Execute(null);
    public async Task AddTopic_IgnoresEmptyInput()
    {
        if (!OperatingSystem.IsWindows()) return;
        var (vm, _) = CreateViewModel();
        vm.NewTopic = "   ";
        vm.AddTopicCommand.Execute(null);
        var client = new Mock<IMqttClient>();
        vm = CreateViewModel(client);
        vm.NewTopic = "   ";
        vm.AddTopicCommand.Execute(null);
        Assert.Empty(vm.TagSubscriptions);
        await vm.AddTopicAsync();
        client.Verify(c => c.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Empty(vm.Subscriptions);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public void RemoveTag_RemovesSelectedSubscription()
    {
        if (!OperatingSystem.IsWindows()) return;
        var vm = CreateViewModel();
        var sub = new TagSubscription { Tag = "t" };
        vm.Subscriptions.Add(sub);
        vm.SelectedSubscription = sub;
        vm.RemoveTagCommand.Execute(null);
        Assert.Empty(vm.Subscriptions);
        Assert.Null(vm.SelectedSubscription);
    public void RemoveTopic_RemovesSelectedSubscription()
    {
        if (!OperatingSystem.IsWindows()) return;
        var (vm, _) = CreateViewModel();
        var tag = new TagSubscription("t");
        vm.Subscriptions.Add(tag);
        vm.SelectedSubscription = tag;
        vm.RemoveTopicCommand.Execute(null);
        var vm = CreateViewModel();
        var sub = new TagSubscription { Tag = "t" };
        vm.TagSubscriptions.Add(sub);
        vm.SelectedSubscription = sub;
        vm.RemoveTopicCommand.Execute(null);
        Assert.Empty(vm.TagSubscriptions);
        Assert.Null(vm.SelectedSubscription);

    }
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
    public void Constructor_PopulatesStylingMetadata()
    {
        if (!OperatingSystem.IsWindows()) return;
        var tag = new TagSubscription("t") { StatusColor = "Red", Icon = "icon.png" };
        var (vm, _) = CreateViewModel(tags: new[] { tag });
        var sub = Assert.Single(vm.Subscriptions);
        Assert.Equal("Red", sub.StatusColor);
        Assert.Equal("icon.png", sub.Icon);
    public void SelectingTag_LoadsAndPersistsMessage()
    {
        if (!OperatingSystem.IsWindows()) return;
        var vm = CreateViewModel();
        var sub1 = new TagSubscription { Tag = "t1", OutgoingMessage = "m1" };
        var sub2 = new TagSubscription { Tag = "t2", OutgoingMessage = "m2" };
        vm.TagSubscriptions.Add(sub1);
        vm.TagSubscriptions.Add(sub2);

        vm.SelectedSubscription = sub1;
        Assert.Equal("m1", vm.SelectedSubscription?.OutgoingMessage);

        vm.SelectedSubscription!.OutgoingMessage = "updated";
        vm.SelectedSubscription = sub2;

        Assert.Equal("updated", sub1.OutgoingMessage);
        Assert.Equal("m2", vm.SelectedSubscription?.OutgoingMessage);
    }
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
    public void TagSubscriptionChanged_RefreshesStyling()
    {
        if (!OperatingSystem.IsWindows()) return;
        var tag = new TagSubscription("t") { StatusColor = "Red" };
        var (vm, service) = CreateViewModel(tags: new[] { tag });
        service.UpdateTagSubscription(new TagSubscription("t") { StatusColor = "Blue" });
        Assert.Equal("Blue", vm.Subscriptions.Single().StatusColor);
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
