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
        var vm = CreateViewModel(client);
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
        var vm = CreateViewModel(client);
        var sub = new TagSubscription { Tag = "t", OutgoingMessage = "m" };
        vm.TagSubscriptions.Add(sub);
        vm.SelectedSubscription = sub;
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
    public void AddTopic_AddsTopicAndClearsInput()
    {
        if (!OperatingSystem.IsWindows()) return;
        var vm = CreateViewModel();
        vm.NewTopic = "topic";
        vm.AddTopicCommand.Execute(null);
        Assert.Contains(vm.TagSubscriptions, t => t.Tag == "topic");
        Assert.Equal(string.Empty, vm.NewTopic);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public void AddTopic_IgnoresEmptyInput()
    {
        if (!OperatingSystem.IsWindows()) return;
        var vm = CreateViewModel();
        vm.NewTopic = "   ";
        vm.AddTopicCommand.Execute(null);
        Assert.Empty(vm.TagSubscriptions);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public void RemoveTopic_RemovesSelectedSubscription()
    {
        if (!OperatingSystem.IsWindows()) return;
        var vm = CreateViewModel();
        var sub = new TagSubscription { Tag = "t" };
        vm.TagSubscriptions.Add(sub);
        vm.SelectedSubscription = sub;
        vm.RemoveTopicCommand.Execute(null);
        Assert.Empty(vm.TagSubscriptions);
        Assert.Null(vm.SelectedSubscription);

    }

    [Fact]
    [TestCategory("WindowsSafe")]
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
}
