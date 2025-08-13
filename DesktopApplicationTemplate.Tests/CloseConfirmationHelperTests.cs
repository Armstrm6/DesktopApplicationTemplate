using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Services;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class CloseConfirmationHelperTests
    {
        [Fact]
        [TestCategory("WindowsSafe")]
        public void Show_ReturnsTrue_WhenSuppressed()
        {
            SettingsViewModel.CloseConfirmationSuppressed = true;
            var logger = new Mock<ILoggingService>();
            var helper = new CloseConfirmationHelper(logger.Object);
            var result = helper.Show();
            Assert.True(result);
            SettingsViewModel.CloseConfirmationSuppressed = false;
            ConsoleTestLogger.LogPass();
        }
    }
}
