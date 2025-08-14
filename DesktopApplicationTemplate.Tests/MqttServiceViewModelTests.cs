using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Helpers;
using Microsoft.Extensions.Options;
using MQTTnet.Client;
using Moq;
using Xunit;
using System.Threading;
using System;
using System.Collections.Generic;
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
            var logger = new Mock<ILoggingService>().Object;
            var options = Options.Create(new DesktopApplicationTemplate.Models.MqttServiceOptions());
            var service = new MqttService(options, logger);
            var vm = new MqttServiceViewModel(helper, service, options, logger);
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
            var options = Options.Create(new DesktopApplicationTemplate.Models.MqttServiceOptions());
            var service = new MqttService(client.Object, options, logger.Object);
            var helper = new SaveConfirmationHelper(logger.Object);
            var vm = new MqttServiceViewModel(helper, service, options, logger.Object) { Host = "127.0.0.1", Port = "1883", ClientId = "c" };

            await vm.ConnectAsync();

            logger.Verify(l => l.Log("MQTT connect start", LogLevel.Debug), Times.Once);
            logger.Verify(l => l.Log("MQTT connect finished", LogLevel.Debug), Times.Once);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("WindowsSafe")]
        public void Constructor_SetsDefaultPlaceholders()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }
            var helper = new SaveConfirmationHelper(new Mock<ILoggingService>().Object);
            var vm = new MqttServiceViewModel(helper);
            Assert.Equal(string.Empty, vm.Host);
            Assert.Equal("1883", vm.Port);
            Assert.Equal("client1", vm.ClientId);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("WindowsSafe")]
        public void PortSetter_RejectsOutOfRange()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }
            var helper = new SaveConfirmationHelper(new Mock<ILoggingService>().Object);
            var vm = new MqttServiceViewModel(helper);
            vm.Port = "70000";
            Assert.Equal("1883", vm.Port);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public async Task ConnectAsync_Disconnects_OnReconfigure()
        {
            var logger = new Mock<ILoggingService>();
            var service = new Mock<MqttService>(new Mock<IMqttClient>().Object, logger.Object);
            service.Setup(s => s.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            service.Setup(s => s.SubscribeAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.CompletedTask);
            service.Setup(s => s.DisconnectAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var helper = new SaveConfirmationHelper(logger.Object);
            var vm = new MqttServiceViewModel(helper, service.Object, logger.Object) { Host = "1.1.1.1", Port = "1883", ClientId = "c" };

            await vm.ConnectAsync();
            vm.Host = "2.2.2.2";
            await vm.ConnectAsync();

            service.Verify(s => s.DisconnectAsync(It.IsAny<CancellationToken>()), Times.Once);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public async Task PublishAsync_ResolvesTokens_AndRoutesToMultipleTopics()
        {
            var logger = new Mock<ILoggingService>();
            var service = new Mock<MqttService>(new Mock<IMqttClient>().Object, logger.Object);
            service.Setup(s => s.PublishAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var tokens = new Dictionary<string, string> { ["Svc"] = "payload" };
            var helper = new SaveConfirmationHelper(logger.Object);
            var vm = new MqttServiceViewModel(helper, service.Object, logger.Object, tokens)
            {
                PublishTopic = "t1;t2",
                PublishMessage = "{Svc.Message}"
            };

            await vm.PublishAsync();

            service.Verify(s => s.PublishAsync("t1", "payload"), Times.Once);
            service.Verify(s => s.PublishAsync("t2", "payload"), Times.Once);


            ConsoleTestLogger.LogPass();
        }
    }
}
