using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;

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
            var client = new Moq.Mock<MQTTnet.Client.IMqttClient>();
            client.Setup(c => c.ConnectAsync(Moq.It.IsAny<MQTTnet.Client.Options.MqttClientOptions>())).Returns(System.Threading.Tasks.Task.CompletedTask);
            client.Setup(c => c.SubscribeAsync(Moq.It.IsAny<string>())).Returns(System.Threading.Tasks.Task.CompletedTask);
            var service = new MqttService(client.Object, logger);
            var vm = new MqttServiceViewModel(service, logger) { Host = "localhost", Port = "1883", ClientId = "c" };

            await vm.ConnectAsync();

            Assert.Contains(logger.Entries, e => e.Message == "MQTT connect start");
            Assert.Contains(logger.Entries, e => e.Message == "MQTT connect finished");

            ConsoleTestLogger.LogPass();
        }
    }
}
