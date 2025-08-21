using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.Models;
using Moq;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class MainViewModelFilterTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void NameFilter_FiltersServices()
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(new StubFileDialogService(), configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object);
            vm.Services.Add(new ServiceViewModel { DisplayName = "HTTP - HTTP1", ServiceType = "HTTP", IsActive = true, Order = 0 });
            vm.Services.Add(new ServiceViewModel { DisplayName = "TCP - TCP1", ServiceType = "TCP", IsActive = true, Order = 1 });

            vm.Filters.NameFilter = "HTTP";

            var visible = vm.FilteredServices.Cast<ServiceViewModel>().ToList();
            Assert.Single(visible);
            Assert.Equal("HTTP - HTTP1", visible[0].DisplayName);
            ConsoleTestLogger.LogPass();
        }
    }
}
