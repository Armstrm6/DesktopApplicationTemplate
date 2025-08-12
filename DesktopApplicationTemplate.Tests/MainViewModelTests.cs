using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.Models;
using Moq;
using System.IO;
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
        public void ClearLogCommand_RemovesLogsAndLogs()
        {
            var logger = new Mock<ILoggingService>();
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object, logger.Object);
            vm.AllLogs.Add(new LogEntry { Message = "Test", Level = LogLevel.Debug });

            vm.ClearLogCommand.Execute(null);

            Assert.Empty(vm.AllLogs);
            logger.Verify(l => l.Log(It.Is<string>(m => m.Contains("Logs cleared")), It.IsAny<LogLevel>()), Times.Once);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void ExportLogCommand_LogsRequest()
        {
            var logger = new Mock<ILoggingService>();
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object, logger.Object);

            vm.ExportLogCommand.Execute(null);

            logger.Verify(l => l.Log(It.Is<string>(m => m.Contains("ExportLog requested")), It.IsAny<LogLevel>()), Times.Once);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void UpdateLogCommand_LogsRequest()
        {
            var logger = new Mock<ILoggingService>();
            var configPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
            var csv = new CsvService(new CsvViewerViewModel(configPath));
            var network = new Mock<INetworkConfigurationService>();
            var networkVm = new NetworkConfigurationViewModel(network.Object);
            var vm = new MainViewModel(csv, networkVm, network.Object, logger.Object);

            vm.UpdateLogCommand.Execute(null);

            logger.Verify(l => l.Log(It.Is<string>(m => m.Contains("UpdateLog requested")), It.IsAny<LogLevel>()), Times.Once);
            ConsoleTestLogger.LogPass();
        }
    }
}
