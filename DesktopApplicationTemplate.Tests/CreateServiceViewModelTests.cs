using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class CreateServiceViewModelTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void SelectedServiceType_GeneratesDefaultName()
        {
            var existing = new[] { "Heartbeat1" };
            var vm = new CreateServiceViewModel(existing);
            vm.SelectedServiceType = "Heartbeat";

            Assert.Equal("Heartbeat2", vm.ServiceName);

            ConsoleTestLogger.LogPass();
        }
    }
}
