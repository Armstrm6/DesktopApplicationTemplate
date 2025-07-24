using DesktopApplicationTemplate.UI.Services;
using FluentFTP;
using Moq;
using System.Net;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class FtpServiceTests
    {
        [Fact]
        public async Task UploadAsync_InvokesClientOperations()
        {
            var client = new Mock<FtpClient>("host", new NetworkCredential("u","p"));
            client.Setup(c => c.Connect());
            client.Setup(c => c.UploadFile("local","remote", FtpRemoteExists.Overwrite));
            client.Setup(c => c.Disconnect());

            var service = new FtpService(client.Object);
            await service.UploadAsync("local","remote");

            client.Verify(c => c.Connect(), Times.Once);
            client.Verify(c => c.UploadFile("local","remote", FtpRemoteExists.Overwrite), Times.Once);
            client.Verify(c => c.Disconnect(), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
