using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Csv;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class MainViewEditServiceTests
{
    public static IEnumerable<object[]> ServiceTypes() =>
        Enum.GetValues<ServiceType>().Select(t => new object[] { t });

    [Theory]
    [MemberData(nameof(ServiceTypes))]
    public void EditService_InvokesCorrectHandler(ServiceType type)
    {
        var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
        var csv = new CsvService(new CsvViewerViewModel(new StubFileDialogService(), configPath));
        var network = new Mock<INetworkConfigurationService>();
        var networkVm = new NetworkConfigurationViewModel(network.Object);
        var mocks = Enum.GetValues<ServiceType>()
            .ToDictionary(t => t, _ => new Mock<IEditServiceHandler>());
        var dict = mocks.ToDictionary(k => k.Key, v => v.Value.Object);
        var vm = TestHelpers.CreateMainViewModel(csv, networkVm, network.Object, dict);
        var service = TestHelpers.CreateService(type, "svc");

        vm.EditServiceCommand.Execute(service);

        foreach (var kvp in mocks)
        {
            if (kvp.Key == type)
            {
                kvp.Value.Verify(h => h.Edit(service), Times.Once);
            }
            else
            {
                kvp.Value.Verify(h => h.Edit(It.IsAny<ServiceListModel>()), Times.Never);
            }
        }
    }

    [Fact]
    public void EditService_UnknownType_NoHandlerCalled()
    {
        var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
        var csv = new CsvService(new CsvViewerViewModel(new StubFileDialogService(), configPath));
        var network = new Mock<INetworkConfigurationService>();
        var networkVm = new NetworkConfigurationViewModel(network.Object);
        var mocks = Enum.GetValues<ServiceType>()
            .ToDictionary(t => t, _ => new Mock<IEditServiceHandler>());
        var dict = mocks.ToDictionary(k => k.Key, v => v.Value.Object);
        var vm = TestHelpers.CreateMainViewModel(csv, networkVm, network.Object, dict);
        var service = new ServiceListModel { Type = (ServiceType)999, DisplayName = "Unknown" };

        vm.EditServiceCommand.Execute(service);

        foreach (var kvp in mocks.Values)
        {
            kvp.Verify(h => h.Edit(It.IsAny<ServiceListModel>()), Times.Never);
        }
    }
}
