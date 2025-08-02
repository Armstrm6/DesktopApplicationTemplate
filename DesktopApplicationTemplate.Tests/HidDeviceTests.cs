using DesktopApplicationTemplate.UI.Services;
using Moq;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Tests
{
    public class HidDeviceTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public async Task SendKeysAsync_Logs()
        {
            var logger = new Mock<ILoggingService>();
            var device = new HidDevice(logger.Object);

            await device.SendKeysAsync("abc", 0, CancellationToken.None);

            logger.Verify(l => l.Log("HID device sending: abc", LogLevel.Debug), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
