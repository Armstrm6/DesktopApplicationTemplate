using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class CreateServiceViewModelTests
    {
        [Theory]
        [InlineData("HID")]
        [InlineData("TCP")]
        [InlineData("HTTP")]
        [InlineData("File Observer")]
        [InlineData("Heartbeat")]
        [InlineData("CSV Creator")]
        [InlineData("SCP")]
        [InlineData("MQTT")]
        [InlineData("FTP")]
        [InlineData("FTP Server")]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void GenerateDefaultName_ReturnsIncrementedName(string type)
        {
            var existing = new[] { $"{type}1" };
            var vm = new CreateServiceViewModel(existing);
            var name = vm.GenerateDefaultName(type);
            Assert.Equal($"{type}2", name);
            ConsoleTestLogger.LogPass();
        }
    }
}
