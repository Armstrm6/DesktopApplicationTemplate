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
        public async Task SendAsync_DelayAndDeviceInvoked()
        {
            var device = new Mock<IHidDevice>();
            device.Setup(d => d.SendKeysAsync("msg", 50, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var delay = new Mock<IDelayService>();
            delay.Setup(d => d.DelayAsync(100, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var logger = new Mock<ILoggingService>();

            var service = new HidService(device.Object, delay.Object, logger.Object);
            await service.SendAsync("msg", 100, 50);

            delay.Verify(d => d.DelayAsync(100, It.IsAny<CancellationToken>()), Times.Once);
            device.Verify(d => d.SendKeysAsync("msg", 50, It.IsAny<CancellationToken>()), Times.Once);
            logger.Verify(l => l.Log("HidService send start", LogLevel.Debug), Times.Once);
            logger.Verify(l => l.Log("HidService send complete", LogLevel.Debug), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
