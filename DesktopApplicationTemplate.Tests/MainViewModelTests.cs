using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Services;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class MainViewModelTests
    {
        [Fact]
        public void GenerateServiceName_IncrementsBasedOnExisting()
        {
            var csv = new CsvService(new CsvViewerViewModel());
            var vm = new MainViewModel(csv);
            vm.Services.Add(new MainViewModel.Services
            {
                DisplayName = "HTTP - HTTP1",
                ServiceType = "HTTP",
                IsActive = false,
                Order = 0
            });
            vm.Services.Add(new MainViewModel.ServiceViewModel
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
    }
}
