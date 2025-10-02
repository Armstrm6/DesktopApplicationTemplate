using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.Core.Services;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class ScpServiceViewModelTests
    {
        [Fact]
        public void DefaultPort_Is22()
        {
            var helper = new SaveConfirmationHelper(new Mock<ILoggingService>().Object);
            var vm = new ScpServiceViewModel(helper);
            Assert.Equal("22", vm.Port);

            ConsoleTestLogger.LogPass();
        }
    }
}
