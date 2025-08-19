using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Moq;
using MQTTnet.Client;
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
        await vm.ConnectAsync();
        client.Verify(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(vm.IsConnected);
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
        var (vm, _) = CreateViewModel(client);
        var sub = new TagSubscription("t");
        vm.Subscriptions.Add(sub);
        vm.SelectedSubscription = sub;
        vm.TestMessage = "m";
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
        var (vm, _) = CreateViewModel(client);
        await vm.PublishTestAsync();
        client.Verify(c => c.PublishAsync(It.IsAny<MQTTnet.MqttApplicationMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public void AddTopic_AddsTopicAndClearsInput()
    {
        if (!OperatingSystem.IsWindows()) return;
        var (vm, _) = CreateViewModel();
        vm.NewTopic = "topic";
        vm.AddTopicCommand.Execute(null);
        Assert.Contains(vm.Subscriptions, s => s.Topic == "topic");
        Assert.Equal(string.Empty, vm.NewTopic);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public void AddTopic_IgnoresEmptyInput()
    {
        if (!OperatingSystem.IsWindows()) return;
        var (vm, _) = CreateViewModel();
        vm.NewTopic = "   ";
        vm.AddTopicCommand.Execute(null);
        Assert.Empty(vm.Subscriptions);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public void RemoveTopic_RemovesSelectedTopic()
    {
        if (!OperatingSystem.IsWindows()) return;
        var (vm, _) = CreateViewModel();
        var tag = new TagSubscription("t");
        vm.Subscriptions.Add(tag);
        vm.SelectedSubscription = tag;
        vm.RemoveTopicCommand.Execute(null);
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
    }
}
