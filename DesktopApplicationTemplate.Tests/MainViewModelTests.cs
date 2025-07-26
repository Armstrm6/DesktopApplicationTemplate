using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
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
            var vm = new MainViewModel(csv);
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

            var servicesPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "services.json");
            Directory.CreateDirectory(Path.GetDirectoryName(servicesPath)!);
            var vm = new MainViewModel(csv, logger.Object, servicesPath);
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
    }
}
