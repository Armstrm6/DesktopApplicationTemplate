using DesktopApplicationTemplate.Core.Services;
using FluentFTP;
using FluentFTP.Client.BaseClient;
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
            var client = new Mock<FtpClient>("host", new NetworkCredential("u","p"));
            client.Setup(c => c.ConnectAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            client.Setup(c => c.UploadFileAsync("local","remote", FtpRemoteExists.Overwrite, It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(FtpStatus.Success));
            client.Setup(c => c.DisconnectAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var service = new FtpService(client.Object);
            await service.UploadAsync("local","remote");

            client.Verify(c => c.ConnectAsync(It.IsAny<CancellationToken>()), Times.Once);
            client.Verify(c => c.UploadFileAsync("local","remote", FtpRemoteExists.Overwrite, It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
            client.Verify(c => c.DisconnectAsync(It.IsAny<CancellationToken>()), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
