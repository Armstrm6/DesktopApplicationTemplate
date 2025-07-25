using DesktopApplicationTemplate.UI.Services;
using FluentFTP;
using Moq;
using System.Threading;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class FtpServiceTests
    {
        [Fact]
        [TestCategory("CodexSafe")]
        public async Task UploadAsync_InvokesClientOperations()
        {
            var client = new Mock<FluentFTP.IAsyncFtpClient>();
            client.SetupGet(c => c.Host).Returns("host");
            client.SetupGet(c => c.Port).Returns(21);
            client.Setup(c => c.Connect(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            client.Setup(c => c.UploadFile("local","remote", FtpRemoteExists.Overwrite, true, FtpVerify.None, null, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(FtpStatus.Success));
            client.Setup(c => c.Disconnect(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var service = new FtpService(client.Object);
            await service.UploadAsync("local","remote");

            client.Verify(c => c.Connect(It.IsAny<CancellationToken>()), Times.Once);
            client.Verify(c => c.UploadFile("local","remote", FtpRemoteExists.Overwrite, It.IsAny<bool>(), It.IsAny<FtpVerify>(), It.IsAny<IProgress<FtpProgress>?>(), It.IsAny<CancellationToken>()), Times.Once);

            client.Verify(c => c.Disconnect(It.IsAny<CancellationToken>()), Times.Once);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        public async Task UploadAsync_LogsStartAndFinish()
        {
            var client = new Mock<FluentFTP.IAsyncFtpClient>();
            client.SetupGet(c => c.Host).Returns("host");
            client.SetupGet(c => c.Port).Returns(21);
            client.Setup(c => c.Connect(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            client.Setup(c => c.UploadFile(It.IsAny<string>(), It.IsAny<string>(), FtpRemoteExists.Overwrite, It.IsAny<bool>(), It.IsAny<FtpVerify>(), It.IsAny<IProgress<FtpProgress>?>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(FtpStatus.Success));
            client.Setup(c => c.Disconnect(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var logger = new TestLogger();
            var service = new FtpService(client.Object, logger);
            await service.UploadAsync("local", "remote");

            Assert.Contains(logger.Entries, e => e.Message.Contains("Starting FTP upload"));
            Assert.Contains(logger.Entries, e => e.Message.Contains("FTP upload completed"));

            ConsoleTestLogger.LogPass();
        }
    }
}
