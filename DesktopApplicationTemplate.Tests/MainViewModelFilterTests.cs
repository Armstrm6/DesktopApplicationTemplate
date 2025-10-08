using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class MainViewModelFilterTests
    {
        [Fact]
        public void NameFilter_FiltersServices()
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(new StubFileDialogService(), configPath));
            var network = new Mock<INetworkConfigurationService>();
            network.Setup(n => n.GetAvailableInterfacesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<string>)Array.Empty<string>());
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object)
            {
                Services =
                {
                    new ServiceListModel { DisplayName = "HTTP - HTTP1", ServiceType = "HTTP", IsActive = true, Order = 0 },
                    new ServiceListModel { DisplayName = "TCP - TCP1", ServiceType = "TCP", IsActive = true, Order = 1 }
                },
                Filters = { NameFilter = "HTTP" }
            };

            var visible = vm.FilteredServices.Cast<ServiceListModel>().ToList();
            Assert.Single(visible);
            Assert.Equal("HTTP - HTTP1", visible[0].DisplayName);
            ConsoleTestLogger.LogPass();
        }
    }
}
