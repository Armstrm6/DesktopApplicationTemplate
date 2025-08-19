using System;
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
    private static MqttEditConnectionViewModel CreateViewModel(Mock<IMqttClient>? clientMock = null)
    {
        var client = clientMock ?? new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult());
        client.Setup(c => c.DisconnectAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var options = Microsoft.Extensions.Options.Options.Create(new MqttServiceOptions());
        var service = new MqttService(client.Object, options, Mock.Of<IMessageRoutingService>(), Mock.Of<ILoggingService>());
        return new MqttEditConnectionViewModel(service, options, Mock.Of<ILoggingService>());
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public async Task UpdateAsync_ReconnectsWithUpdatedOptions()
    {
        if (!OperatingSystem.IsWindows()) return;
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
    [TestCategory("WindowsSafe")]
    public async Task UnsubscribeAsync_Disconnects()
    {
        if (!OperatingSystem.IsWindows()) return;
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult());
        var options = Microsoft.Extensions.Options.Options.Create(new MqttServiceOptions());
        var service = new MqttService(client.Object, options, Mock.Of<IMessageRoutingService>(), Mock.Of<ILoggingService>());
        var vm = new MqttEditConnectionViewModel(service, options);
        await vm.UnsubscribeAsync();
        client.Verify(c => c.DisconnectAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [TestCategory("WindowsSafe")]
    public void Cancel_DoesNotModifyOptions()
    {
        if (!OperatingSystem.IsWindows()) return;
        var client = new Mock<IMqttClient>();
        client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MqttClientConnectResult());
        var options = Microsoft.Extensions.Options.Options.Create(new MqttServiceOptions { Host = "original" });
        var service = new MqttService(client.Object, options, Mock.Of<IMessageRoutingService>(), Mock.Of<ILoggingService>());
        var vm = new MqttEditConnectionViewModel(service, options);
        vm.Host = "changed";
        vm.Cancel();
        Assert.Equal("original", options.Value.Host);
    }
}
