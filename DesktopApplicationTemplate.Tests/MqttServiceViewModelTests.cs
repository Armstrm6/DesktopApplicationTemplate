using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Helpers;
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
        [TestCategory("WindowsSafe")]
        public void AddTopicCommand_AddsTopic()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }
            var helper = new SaveConfirmationHelper(new Mock<ILoggingService>().Object);
            var vm = new MqttServiceViewModel(helper);
            vm.NewTopic = "test/topic";
            vm.AddTopicCommand.Execute(null);
            Assert.Contains("test/topic", vm.Topics);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public async Task ConnectAsync_LogsLifecycle()
        {
            var logger = new Mock<ILoggingService>();
            var client = new Mock<IMqttClient>();
            client.Setup(c => c.ConnectAsync(It.IsAny<MqttClientOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MqttClientConnectResult());
            client.Setup(c => c.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MqttClientSubscribeResult(0, Array.Empty<MqttClientSubscribeResultItem>(), string.Empty, Array.Empty<MqttUserProperty>()));
            var service = new MqttService(client.Object, logger.Object);
            var helper = new SaveConfirmationHelper(logger.Object);
            var vm = new MqttServiceViewModel(helper, service, logger.Object) { Host = "127.0.0.1", Port = "1883", ClientId = "c" };

            await vm.ConnectAsync();

            logger.Verify(l => l.Log("MQTT connect start", LogLevel.Debug), Times.Once);
            logger.Verify(l => l.Log("MQTT connect finished", LogLevel.Debug), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
