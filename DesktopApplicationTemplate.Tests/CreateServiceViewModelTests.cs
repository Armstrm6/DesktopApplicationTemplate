using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class CreateServiceViewModelTests
    {
        [Theory]
        [InlineData("TCP")]
        [InlineData("HTTP")]
        [InlineData("CSV Creator")]
        [InlineData("SCP")]
        [InlineData("MQTT")]
        [InlineData("FTP Server")]
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
