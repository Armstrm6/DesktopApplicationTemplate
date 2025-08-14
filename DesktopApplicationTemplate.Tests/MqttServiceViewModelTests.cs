using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Moq;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Publishing;
using MQTTnet.Packets;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class MqttServiceViewModelTests
{
    private static MqttServiceViewModel CreateViewModel(
        Mock<IMqttClient>? clientMock = null,
        Mock<IMessageRoutingService>? routingMock = null)
    {
        var logger = Mock.Of<ILoggingService>();
        var options = Microsoft.Extensions.Options.Options.Create(new MqttServiceOptions());
        var routing = routingMock ?? new Mock<IMessageRoutingService>();
        var client = clientMock ?? new Mock<IMqttClient>();
        client.Setup(c => c.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientPublishResult(null, MqttClientPublishReasonCode.Success, null!, Array.Empty<MqttUserProperty>()));
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult());
        var service = new MqttService(client.Object, options, routing.Object, logger);
        var helper = new SaveConfirmationHelper(logger);
        return new MqttServiceViewModel(service, routing.Object, helper, options, logger);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public void AddTopicCommand_AddsTopic()
    {
        if (!OperatingSystem.IsWindows()) return;
        var vm = CreateViewModel();
        vm.NewTopic = "test/topic";
        vm.AddTopicCommand.Execute(null);
        Assert.Contains("test/topic", vm.Topics);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public void AddMessageCommand_AddsPair()
    {
        if (!OperatingSystem.IsWindows()) return;
        var vm = CreateViewModel();
        vm.NewEndpoint = "endpoint";
        vm.NewMessage = "payload";
        vm.AddMessageCommand.Execute(null);
        Assert.Single(vm.Messages);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public void RemoveMessageCommand_RemovesSelectedPair()
    {
        if (!OperatingSystem.IsWindows()) return;
        var vm = CreateViewModel();
        vm.Messages.Add(new MqttEndpointMessage { Endpoint = "t", Message = "m" });
        vm.SelectedMessage = vm.Messages.First();
        vm.RemoveMessageCommand.Execute(null);
        Assert.Empty(vm.Messages);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public async Task ConnectAsync_SetsIsConnected()
    {
        if (!OperatingSystem.IsWindows()) return;
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult());
        var vm = CreateViewModel(client);
        await vm.ConnectAsync();
        Assert.True(vm.IsConnected);
        client.Verify(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public async Task PublishSelectedAsync_ResolvesTokens()
    {
        if (!OperatingSystem.IsWindows()) return;
        var routing = new Mock<IMessageRoutingService>();
        routing.Setup(r => r.ResolveTokens("endpoint")).Returns("resolvedEndpoint");
        routing.Setup(r => r.ResolveTokens("payload")).Returns("resolvedPayload");
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientPublishResult(null, MqttClientPublishReasonCode.Success, null!, Array.Empty<MqttUserProperty>()));
        var vm = CreateViewModel(client, routing);
        vm.Messages.Add(new MqttEndpointMessage { Endpoint = "endpoint", Message = "payload" });
        vm.SelectedMessage = vm.Messages.First();
        await vm.PublishSelectedAsync();
        routing.Verify(r => r.ResolveTokens("endpoint"), Times.Once);
        routing.Verify(r => r.ResolveTokens("payload"), Times.Once);
        client.Verify(c => c.PublishAsync(It.Is<MqttApplicationMessage>(m => m.Topic == "resolvedEndpoint" && m.ConvertPayloadToString() == "resolvedPayload"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public void PortSetter_RejectsOutOfRange()
    {
        if (!OperatingSystem.IsWindows()) return;
        var vm = CreateViewModel();
        vm.Port = 70000;
        Assert.Contains("Port must be 1-65535", vm.GetErrors(nameof(vm.Port)).Cast<string>());
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public void HostSetter_InvalidAddsError()
    {
        if (!OperatingSystem.IsWindows()) return;
        var vm = CreateViewModel();
        vm.Host = "invalid_host";
        Assert.Contains("Invalid host", vm.GetErrors(nameof(vm.Host)).Cast<string>());
    }
}

