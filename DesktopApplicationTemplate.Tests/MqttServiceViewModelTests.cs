using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Helpers;
using MQTTnet.Client;
using Moq;
using Xunit;
using System.Threading;
using System;
using MQTTnet.Packets;
using DesktopApplicationTemplate.UI.Models;

using MQTTnet;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var vm = new MqttServiceViewModel(helper, new MqttServiceOptions());
            vm.NewTopic = "test/topic";
            vm.AddTopicCommand.Execute(null);
            Assert.Contains("test/topic", vm.Topics);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("WindowsSafe")]
        public void AddEndpointMessageCommand_AddsPair()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }
            var helper = new SaveConfirmationHelper(new Mock<ILoggingService>().Object);
            var vm = new MqttServiceViewModel(helper);
            vm.AddEndpointMessageCommand.Execute(null);
            Assert.Single(vm.EndpointMessages);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("WindowsSafe")]
        public void RemoveEndpointMessageCommand_RemovesSelectedPair()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }
            var helper = new SaveConfirmationHelper(new Mock<ILoggingService>().Object);
            var vm = new MqttServiceViewModel(helper);
            vm.EndpointMessages.Add(new EndpointMessagePair { Endpoint = "topic", Message = "msg" });
            vm.SelectedEndpointMessage = vm.EndpointMessages[0];
            vm.RemoveEndpointMessageCommand.Execute(null);
            Assert.Empty(vm.EndpointMessages);

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
            var options = new MqttServiceOptions { Host = "127.0.0.1", Port = 1883, ClientId = "c" };
            var service = new MqttService(client.Object, options, logger.Object);
            var helper = new SaveConfirmationHelper(logger.Object);
            var vm = new MqttServiceViewModel(helper, options, service, logger.Object);

            await vm.ConnectAsync();

            logger.Verify(l => l.Log("MQTT connect start", LogLevel.Debug), Times.Once);
            logger.Verify(l => l.Log("MQTT connect finished", LogLevel.Debug), Times.Once);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        public void HostChange_Disconnects_WhenConnected()
        {
            var logger = new Mock<ILoggingService>();
            var client = new Mock<IMqttClient>();
            client.SetupGet(c => c.IsConnected).Returns(true);
            client.Setup(c => c.DisconnectAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

            var options = new MqttServiceOptions { Host = "127.0.0.1" };
            var service = new MqttService(client.Object, options, logger.Object);
            var helper = new SaveConfirmationHelper(logger.Object);
            var vm = new MqttServiceViewModel(helper, options, service, logger.Object);

            vm.Host = "192.168.0.1";

            client.Verify(c => c.DisconnectAsync(It.IsAny<CancellationToken>()), Times.Once);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public async Task PublishSelectedAsync_ResolvesTokens()
        {
            var logger = new Mock<ILoggingService>();
            var client = new Mock<IMqttClient>();
            string? publishedTopic = null;
            string? publishedPayload = null;
            client.Setup(c => c.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
                .Callback<MqttApplicationMessage, CancellationToken>((m, _) =>
                {
                    publishedTopic = m.Topic;
                    publishedPayload = Encoding.UTF8.GetString(m.PayloadSegment);
                })
                .ReturnsAsync(new MqttClientPublishResult(null, MqttClientPublishReasonCode.Success, null!, Array.Empty<MqttUserProperty>()));

            var options = new MqttServiceOptions { Host = "host", Port = 1883, ClientId = "id" };
            var service = new MqttService(client.Object, options, logger.Object);
            var helper = new SaveConfirmationHelper(logger.Object);
            var vm = new MqttServiceViewModel(helper, options, service, logger.Object);

            vm.NewEndpoint = "topic/{CLIENTID}";
            vm.NewMessage = "hello {HOST}:{PORT}";
            vm.AddMessageCommand.Execute(null);
            vm.SelectedMessage = vm.Messages.First();

            await vm.PublishSelectedAsync();

            Assert.Equal("topic/id", publishedTopic);
            Assert.Equal("hello host:1883", publishedPayload);
            ConsoleTestLogger.LogPass();
        }
    }
}
