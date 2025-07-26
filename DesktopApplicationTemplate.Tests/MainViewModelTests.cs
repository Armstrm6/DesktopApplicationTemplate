using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Moq;
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
            var csv = new CsvService(new CsvViewerViewModel(Path.GetTempFileName()));
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
            var csv = new CsvService(new CsvViewerViewModel(Path.GetTempFileName()));
            var vm = new MainViewModel(csv, logger.Object);
            var service = new ServiceViewModel { DisplayName = "HTTP - HTTP1", ServiceType = "HTTP" };
            vm.Services.Add(service);
            vm.SelectedService = service;

            vm.RemoveServiceCommand.Execute(null);

            logger.Verify(l => l.Log(It.Is<string>(m => m.Contains("Removing service")), It.IsAny<LogLevel>()), Times.Once);
            logger.Verify(l => l.Log(It.Is<string>(m => m.Contains("Service removed")), It.IsAny<LogLevel>()), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
