using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.ViewModels;
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
            var result = CloseConfirmationHelper.Show();
            Assert.True(result);
            SettingsViewModel.CloseConfirmationSuppressed = false;
            ConsoleTestLogger.LogPass();
        }
    }
}
