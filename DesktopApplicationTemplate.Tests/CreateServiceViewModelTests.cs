using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Core.Converters;
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
            var existing = new[] { $"{ServiceTypeJsonConverter.ToLegacyString(type)}1" };
            var vm = new CreateServiceViewModel(existing);
            var name = vm.GenerateDefaultName(type);
            Assert.Equal($"{ServiceTypeJsonConverter.ToLegacyString(type)}2", name);
            ConsoleTestLogger.LogPass();
        }
    }
}
