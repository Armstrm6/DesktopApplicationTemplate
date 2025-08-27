using DesktopApplicationTemplate.UI.Services;
using Moq;
using Renci.SshNet;
using System.Threading.Tasks;
using System.IO;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class ScpServiceTests
    {
        [Fact]
        public async Task UploadAsync_InvokesClientOperations()
        {
            var client = new Mock<IScpClient>();
            client.Setup(c => c.ConnectionInfo).Returns(new ConnectionInfo("host", 22, "u", new PasswordAuthenticationMethod("u", "p")));
            client.Setup(c => c.Connect());
            client.Setup(c => c.Upload(It.IsAny<System.IO.Stream>(), "remote"));
            client.Setup(c => c.Disconnect());

            var service = new ScpService(client.Object);
            var tempFile = System.IO.Path.GetTempFileName();
            try
            {
                System.IO.File.WriteAllText(tempFile, "data");
                await service.UploadAsync(tempFile, "remote");
            }
            finally
            {
                System.IO.File.Delete(tempFile);
            }

            client.Verify(c => c.Connect(), Times.Once);
            client.Verify(c => c.Upload(It.IsAny<System.IO.Stream>(), "remote"), Times.Once);
            client.Verify(c => c.Disconnect(), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
