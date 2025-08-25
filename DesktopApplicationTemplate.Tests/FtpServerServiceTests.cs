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
        public async Task StopAsync_StopsHost()
        {
            var host = new Mock<IFtpServerHost>();
            var logger = new Mock<ILogger<FtpServerService>>();
            var service = new FtpServerService(host.Object, logger.Object);

            await service.StopAsync(CancellationToken.None);

            host.Verify(h => h.StopAsync(It.IsAny<CancellationToken>()), Times.Once);
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
