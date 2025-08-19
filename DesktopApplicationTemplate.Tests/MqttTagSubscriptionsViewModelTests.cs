using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
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
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public async Task TestTagEndpointCommand_Publishes_WhenValid()
    {
        if (!OperatingSystem.IsWindows()) return;
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult());
        var publishCalled = false;
        client.Setup(c => c.PublishAsync(It.Is<MQTTnet.MqttApplicationMessage>(m => m.Topic == "t"), It.IsAny<CancellationToken>()))
            .Callback(() => publishCalled = true)
            .ReturnsAsync(new MqttClientPublishResult(null, MqttClientPublishReasonCode.Success, null!, Array.Empty<MQTTnet.Packets.MqttUserProperty>()));
        var vm = CreateViewModel(client);
        var sub = new TagSubscription { Tag = "tag", Endpoint = "t", OutgoingMessage = "m" };
        vm.Subscriptions.Add(sub);
        await ((AsyncRelayCommand<TagSubscription>)vm.TestTagEndpointCommand).ExecuteAsync(sub);
        Assert.True(publishCalled);
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
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public void AddTag_IgnoresEmptyInput()
    {
        if (!OperatingSystem.IsWindows()) return;
        var vm = CreateViewModel();
        vm.NewTag = "   ";
        vm.AddTagCommand.Execute(null);
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
    }
}
