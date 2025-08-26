using System;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Service.Services;
using FubarDev.FtpServer;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class FtpServerServiceTests
    {
        [Fact]
        public async Task StartAsync_StartsHost()
        {
            var host = new Mock<IFtpServerHost>();
            var logger = new Mock<ILogger<FtpServerService>>();
            var service = new FtpServerService(host.Object, logger.Object);

            await service.StartAsync(CancellationToken.None);

            host.Verify(h => h.StartAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public async Task StartAsync_LogsError_WhenHostThrows()
        {
            var host = new Mock<IFtpServerHost>();
            var logger = new Mock<ILogger<FtpServerService>>();
            host.Setup(h => h.StartAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException());
            var service = new FtpServerService(host.Object, logger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.StartAsync(CancellationToken.None));
            logger.Verify(l => l.Log(
                Microsoft.Extensions.Logging.LogLevel.Error,
                FtpServerService.EventIds.StartFailed,
                It.IsAny<It.IsAnyType>(),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        public async Task StopAsync_StopsHost()
        {
            var host = new Mock<IFtpServerHost>();
            var logger = new Mock<ILogger<FtpServerService>>();
            var service = new FtpServerService(host.Object, logger.Object);

            await service.StopAsync(CancellationToken.None);

            host.Verify(h => h.StopAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public async Task StopAsync_LogsError_WhenHostThrows()
        {
            var host = new Mock<IFtpServerHost>();
            var logger = new Mock<ILogger<FtpServerService>>();
            host.Setup(h => h.StopAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException());
            var service = new FtpServerService(host.Object, logger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.StopAsync(CancellationToken.None));
            logger.Verify(l => l.Log(
                Microsoft.Extensions.Logging.LogLevel.Error,
                FtpServerService.EventIds.StopFailed,
                It.IsAny<It.IsAnyType>(),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        public void RaiseFileReceived_TriggersEvent()
        {
            var host = new Mock<IFtpServerHost>();
            var logger = new Mock<ILogger<FtpServerService>>();
            var service = new FtpServerService(host.Object, logger.Object);
            FtpTransferEventArgs? captured = null;
            service.FileReceived += (_, e) => captured = e;

            service.RaiseFileReceived("test.txt", 10);

            captured.Should().NotBeNull();
            captured!.Path.Should().Be("test.txt");
            captured.IsUpload.Should().BeTrue();
        }

        [Fact]
        public void RaiseFileSent_TriggersEvent()
        {
            var host = new Mock<IFtpServerHost>();
            var logger = new Mock<ILogger<FtpServerService>>();
            var service = new FtpServerService(host.Object, logger.Object);
            FtpTransferEventArgs? captured = null;
            service.FileSent += (_, e) => captured = e;

            service.RaiseFileSent("test.txt", 10);

            captured.Should().NotBeNull();
            captured!.Path.Should().Be("test.txt");
            captured.IsUpload.Should().BeFalse();
        }
    }
}
