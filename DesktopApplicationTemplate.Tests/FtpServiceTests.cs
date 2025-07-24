using DesktopApplicationTemplate.UI.Services;
using FluentFTP;
using Moq;
using System.Net;
using System.Threading;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class FtpServiceTests
    {
        [Fact]
        public async Task UploadAsync_InvokesClientOperations()
        {
            var client = new Mock<AsyncFtpClient>("host", 21, new NetworkCredential("u","p"));
            client.Setup(c => c.Connect(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            client.Setup(c => c.UploadFile("local","remote", FtpRemoteExists.Overwrite, true, FtpVerify.None, It.IsAny<CancellationToken>())).Returns(Task.FromResult(FtpStatus.Success));
            client.Setup(c => c.Disconnect(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var service = new FtpService(client.Object);
            await service.UploadAsync("local","remote");

            client.Verify(c => c.Connect(It.IsAny<CancellationToken>()), Times.Once);
            client.Verify(c => c.UploadFile("local","remote", FtpRemoteExists.Overwrite, true, FtpVerify.None, It.IsAny<CancellationToken>()), Times.Once);
            client.Verify(c => c.Disconnect(It.IsAny<CancellationToken>()), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
