using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class ScpServiceViewModelTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        public void DefaultPort_Is22()
        {
            var logger = new Mock<ILoggingService>();
            var vm = new ScpServiceViewModel(new SaveConfirmationHelper(logger.Object));
            Assert.Equal("22", vm.Port);

            ConsoleTestLogger.LogPass();
        }
    }
}
