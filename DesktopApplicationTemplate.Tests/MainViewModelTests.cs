using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Moq;
using System.IO;
using System.Linq;
using DesktopApplicationTemplate.Models;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class MainViewModelTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void GenerateServiceName_IncrementsBasedOnExisting()
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object);
            vm.Services.Add(new ServiceViewModel
            {
                DisplayName = "HTTP - HTTP1",
                ServiceType = "HTTP",
                IsActive = false,
                Order = 0
            });
            vm.Services.Add(new ServiceViewModel
            {
                DisplayName = "HTTP - HTTP3",
                ServiceType = "HTTP",
                IsActive = false,
                Order = 1
            });

            string next = vm.GenerateServiceName("HTTP");

            Assert.Equal("HTTP4", next);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void RemoveServiceCommand_LogsLifecycle()
        {
            var logger = new Mock<ILoggingService>();
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);

            var servicesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "services.json");
            Directory.CreateDirectory(Path.GetDirectoryName(servicesPath)!);
            var vm = new MainViewModel(csv, networkVm, network.Object, logger.Object, servicesPath);
            var service = new ServiceViewModel { DisplayName = "HTTP - HTTP1", ServiceType = "HTTP" };
            vm.Services.Add(service);
            vm.SelectedService = service;

            vm.RemoveServiceCommand.Execute(null);

            if (File.Exists(servicesPath))
                File.Delete(servicesPath);

            logger.Verify(l => l.Log(It.Is<string>(m => m.Contains("Removing service")), It.IsAny<LogLevel>()), Times.Once);
            logger.Verify(l => l.Log(It.Is<string>(m => m.Contains("Service removed")), It.IsAny<LogLevel>()), Times.Once);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void ClearLogs_RemovesEntries()
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object);
            var svc = new ServiceViewModel { DisplayName = "HTTP - HTTP1", ServiceType = "HTTP" };
            svc.Logs.Add(new LogEntry { Message = "test" });
            vm.Services.Add(svc);
            vm.SelectedService = svc;

            vm.ClearLogs();

            Assert.Empty(svc.Logs);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void ExportDisplayedLogs_WritesFile()
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object);
            var svc = new ServiceViewModel { DisplayName = "HTTP - HTTP1", ServiceType = "HTTP" };
            svc.Logs.Add(new LogEntry { Message = "first" });
            vm.Services.Add(svc);
            vm.SelectedService = svc;

            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
            vm.ExportDisplayedLogs(path);
            var lines = File.ReadAllLines(path);

            Assert.Single(lines);
            File.Delete(path);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void RefreshLogs_RaisesPropertyChanged()
        {
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object);
            bool raised = false;
            vm.PropertyChanged += (s, e) => { if (e.PropertyName == "DisplayLogs") raised = true; };

            vm.RefreshLogs();

            Assert.True(raised);
            ConsoleTestLogger.LogPass();
        }
    }
}
