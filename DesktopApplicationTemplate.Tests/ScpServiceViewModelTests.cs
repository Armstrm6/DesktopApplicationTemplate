using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class ScpServiceViewModelTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        public void DefaultPort_Is22()
        {
            var vm = new ScpServiceViewModel();
            Assert.Equal("22", vm.Port);

            ConsoleTestLogger.LogPass();
        }
    }
}
