using DesktopApplicationTemplate.UI.Services;
using Moq;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Tests
{
    public class HidServiceTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public async Task SendAsync_InvokesDeviceAndLogs()
        {
            var device = new Mock<IHidDevice>();
            device.Setup(d => d.SendKeystrokesAsync("abc", 1, 2, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var logger = new Mock<ILoggingService>();
            var service = new HidService(device.Object, logger.Object);

            await service.SendAsync("abc", 1, 2);

            device.Verify(d => d.SendKeystrokesAsync("abc", 1, 2, It.IsAny<CancellationToken>()), Times.Once);
            logger.Verify(l => l.Log("HidService send start", LogLevel.Debug), Times.Once);
            logger.Verify(l => l.Log("HidService send finished", LogLevel.Debug), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
