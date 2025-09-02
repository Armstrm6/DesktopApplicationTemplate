using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Moq;
using MQTTnet;
using MQTTnet.Client;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class MqttEditConnectionViewModelTests
{
    private static MqttEditConnectionViewModel CreateViewModel(Mock<IMqttClient>? clientMock = null, bool isConnected = false)
    {
        var client = clientMock ?? new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult());
        client.Setup(c => c.DisconnectAsync(It.IsAny<MqttClientDisconnectOptions?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        client.SetupGet(c => c.IsConnected).Returns(isConnected);
        var options = Microsoft.Extensions.Options.Options.Create(new MqttServiceOptions());
        var service = new MqttService(client.Object, options, Mock.Of<IMessageRoutingService>(), Mock.Of<ILoggingService>());
        return new MqttEditConnectionViewModel(service, options, Mock.Of<ILoggingService>());
    }

    [Fact]
    public async Task UpdateAsync_ReconnectsWithUpdatedOptions()
    {
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult());
        var options = Microsoft.Extensions.Options.Options.Create(new MqttServiceOptions());
        var service = new MqttService(client.Object, options, Mock.Of<IMessageRoutingService>(), Mock.Of<ILoggingService>());
        var vm = new MqttEditConnectionViewModel(service, options);
        vm.Host = "example.com";
        vm.Port = 1234;
        vm.ClientId = "cid";
        await vm.UpdateAsync();
        Assert.Equal("example.com", options.Value.Host);
        Assert.Equal(1234, options.Value.Port);
        client.Verify(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleSubscriptionAsync_Disconnects()
    {
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult());
        client.Setup(c => c.DisconnectAsync(It.IsAny<MqttClientDisconnectOptions?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var vm = CreateViewModel(client, true);
        var closed = false;
        vm.RequestClose += (_, _) => closed = true;
        await vm.ToggleSubscriptionAsync();
        client.Verify(c => c.DisconnectAsync(It.IsAny<MqttClientDisconnectOptions?>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(closed);
    }

    [Fact]
    public void Cancel_DoesNotModifyOptions()
    {
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult());
        var options = Microsoft.Extensions.Options.Options.Create(new MqttServiceOptions { Host = "original" });
        var service = new MqttService(client.Object, options, Mock.Of<IMessageRoutingService>(), Mock.Of<ILoggingService>());
        var vm = new MqttEditConnectionViewModel(service, options);
        vm.Host = "changed";
        var closed = false;
        vm.RequestClose += (_, _) => closed = true;
        vm.Cancel();
        Assert.Equal("original", options.Value.Host);
        Assert.True(closed);
    }

    [Fact]
    public void Host_Invalid_AddsError()
    {
        var vm = CreateViewModel();
        var original = vm.Host;
        vm.Host = "bad host";
        Assert.Equal(original, vm.Host);
        Assert.True(vm.HasErrors);
        Assert.Contains("Invalid host", vm.GetErrors(nameof(vm.Host)).Cast<string>());
    }

    [Fact]
    public void Port_Invalid_AddsError()
    {
        var vm = CreateViewModel();
        var original = vm.Port;
        vm.Port = 70000;
        Assert.Equal(original, vm.Port);
        Assert.True(vm.HasErrors);
        Assert.Contains("Port must be 1-65535", vm.GetErrors(nameof(vm.Port)).Cast<string>());
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAllOptionsAndRaisesRequestClose()
    {
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult());
        var options = Microsoft.Extensions.Options.Options.Create(new MqttServiceOptions());
        var service = new MqttService(client.Object, options, Mock.Of<IMessageRoutingService>(), Mock.Of<ILoggingService>());
        var vm = new MqttEditConnectionViewModel(service, options);
        bool closed = false;
        vm.RequestClose += (_, _) => closed = true;
        vm.Host = "example.com";
        vm.Port = 8883;
        vm.ClientId = "cid";
        vm.Username = "user";
        vm.Password = "pass";
        vm.ConnectionType = MqttConnectionType.WebSocket;
        vm.WebSocketPath = "/mqtt";
        await vm.UpdateAsync();
        Assert.Equal("example.com", options.Value.Host);
        Assert.Equal(8883, options.Value.Port);
        Assert.Equal("cid", options.Value.ClientId);
        Assert.Equal("user", options.Value.Username);
        Assert.Equal("pass", options.Value.Password);
        Assert.Equal(MqttConnectionType.WebSocket, options.Value.ConnectionType);
        Assert.Equal("/mqtt", options.Value.WebSocketPath);
        Assert.True(closed);
    }

    [Fact]
    [Fact]
    public void SubscriptionButtonText_ReflectsConnectionState()
    {
        var vm = CreateViewModel();
        Assert.Equal("Subscribe", vm.SubscriptionButtonText);
        vm.GetType().GetProperty("IsConnected")!.SetValue(vm, true);
        Assert.Equal("Unsubscribe", vm.SubscriptionButtonText);
    }
}
