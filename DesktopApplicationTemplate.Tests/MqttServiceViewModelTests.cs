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
    }
}
