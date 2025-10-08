using System.Collections.Generic;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class NetworkConfigurationViewModelTests
    {
        [Fact]
        public async Task LoadAndApplyConfiguration_UsesService()
        {
            var service = new Mock<INetworkConfigurationService>();
            service.Setup(s => s.GetAvailableInterfacesAsync(default)).ReturnsAsync((IReadOnlyList<string>)new List<string> { "Adapter A", "Adapter B" });
            service.Setup(s => s.GetConfigurationAsync(default)).ReturnsAsync(new NetworkConfiguration
            {
                InterfaceName = "Adapter B",
                IpAddress = "2.2.2.2",
                SubnetMask = "255.255.255.0",
                Gateway = "2.2.2.254",
                DnsPrimary = "8.8.4.4"
            });

            var vm = new NetworkConfigurationViewModel(service.Object, null);
            await vm.LoadAsync();

            Assert.Equal("2.2.2.2", vm.IpAddress);
            Assert.Equal("Adapter B", vm.SelectedInterface);

            vm.IpAddress = "3.3.3.3";
            vm.SelectedInterface = "Adapter A";
            await vm.ApplyAsync();

            service.Verify(s => s.ApplyConfigurationAsync(It.Is<NetworkConfiguration>(c => c.IpAddress == "3.3.3.3" && c.InterfaceName == "Adapter A"), default), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
