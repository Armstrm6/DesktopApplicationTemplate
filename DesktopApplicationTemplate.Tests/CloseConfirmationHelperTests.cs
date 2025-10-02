using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.Core.Services;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    [Collection("NonParallel")]
    public class CloseConfirmationHelperTests
    {
        [Fact]
        public void Show_ReturnsTrue_WhenSuppressed()
        {
            var logger = new Mock<ILoggingService>();
            var helper = new CloseConfirmationHelper(logger.Object)
            {
                CloseConfirmationSuppressed = true
            };
            var result = helper.Show();
            Assert.True(result);
            helper.CloseConfirmationSuppressed = false;
            ConsoleTestLogger.LogPass();
        }
    }
}
