using DesktopApplicationTemplate.UI;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Csv;
using DesktopApplicationTemplate.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object, new Dictionary<ServiceType, IEditServiceHandler>());
            var svc1 = TestHelpers.CreateService(ServiceType.Http, "HTTP1");
            svc1.IsActive = true;
            svc1.Order = 0;
            var svc2 = TestHelpers.CreateService(ServiceType.Tcp, "TCP1");
            svc2.IsActive = true;
            svc2.Order = 1;
            vm.Services.Add(svc1);
            vm.Services.Add(svc2);
            vm.Filters.NameFilter = "HTTP";

            var visible = vm.FilteredServices.Cast<ServiceListModel>().ToList();
            Assert.Single(visible);
            Assert.Equal("HTTP - HTTP1", visible[0].DisplayName);
            ConsoleTestLogger.LogPass();
        }
    }
}
