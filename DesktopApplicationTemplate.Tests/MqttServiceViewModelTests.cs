using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using MQTTnet.Client;
using Moq;
using Xunit;
using System.Threading;
using System;
using MQTTnet.Packets;

namespace DesktopApplicationTemplate.Tests
{
    public class MqttServiceViewModelTests
    {
        [Fact]
        [TestCategory("WindowsOnly")]
        public void AddTopicCommand_AddsTopic()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }
            var vm = new MqttServiceViewModel();
            vm.NewTopic = "test/topic";
            vm.AddTopicCommand.Execute(null);
            Assert.Contains("test/topic", vm.Topics);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        public async Task ConnectAsync_LogsLifecycle()
        {
            var logger = new TestLogger();
            var client = new Mock<IMqttClient>();
            client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MqttClientConnectResult());
            client.Setup(c => c.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MqttClientSubscribeResult(0, Array.Empty<MqttClientSubscribeResultItem>(), string.Empty, Array.Empty<MqttUserProperty>()));
            var service = new MqttService(client.Object, logger);
            var vm = new MqttServiceViewModel(service, logger) { Host = "localhost", Port = "1883", ClientId = "c" };

            await vm.ConnectAsync();

            Assert.Contains(logger.Entries, e => e.Message == "MQTT connect start");
            Assert.Contains(logger.Entries, e => e.Message == "MQTT connect finished");

            ConsoleTestLogger.LogPass();
        }
    }
}
