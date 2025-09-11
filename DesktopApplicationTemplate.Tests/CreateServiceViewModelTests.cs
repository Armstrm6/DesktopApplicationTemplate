using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.Models;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class CreateServiceViewModelTests
    {
        [Theory]
        [InlineData(ServiceType.Tcp)]
        [InlineData(ServiceType.Http)]
        [InlineData(ServiceType.Csv)]
        [InlineData(ServiceType.Scp)]
        [InlineData(ServiceType.Mqtt)]
        [InlineData(ServiceType.Ftp)]
        public void GenerateDefaultName_ReturnsIncrementedName(ServiceType type)
        {
            var existing = new[] { $"{type.ToLegacyString()}1" };
            var vm = new CreateServiceViewModel(existing);
            var name = vm.GenerateDefaultName(type);
            Assert.Equal($"{type.ToLegacyString()}2", name);
            ConsoleTestLogger.LogPass();
        }
    }
}
