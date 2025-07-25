using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Services;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class MainViewModelTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        public void GenerateServiceName_IncrementsBasedOnExisting()
        {
            var csv = new CsvService(new CsvViewerViewModel());
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
        public void RemoveServiceCommand_LogsLifecycle()
        {
            var logger = new TestLogger();
            var csv = new CsvService(new CsvViewerViewModel());
            var vm = new MainViewModel(csv, logger);
            var service = new ServiceViewModel { DisplayName = "HTTP - HTTP1", ServiceType = "HTTP" };
            vm.Services.Add(service);
            vm.SelectedService = service;

            vm.RemoveServiceCommand.Execute(null);

            Assert.Contains(logger.Entries, e => e.Message.Contains("Removing service"));
            Assert.Contains(logger.Entries, e => e.Message.Contains("Service removed"));

            ConsoleTestLogger.LogPass();
        }
    }
}
