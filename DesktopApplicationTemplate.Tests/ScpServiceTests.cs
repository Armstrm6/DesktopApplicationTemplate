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
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public async Task UploadAsync_InvokesClientOperations()
        {
            var client = new Mock<IScpClient>();
            client.Setup(c => c.ConnectionInfo).Returns(new ConnectionInfo("host", 22, "u", new PasswordAuthenticationMethod("u", "p")));
            client.Setup(c => c.Connect());
            client.Setup(c => c.Upload(It.IsAny<System.IO.Stream>(), "remote"));
            client.Setup(c => c.Disconnect());

            var service = new ScpService(client.Object);
            var localFile = Path.GetTempFileName();
            await File.WriteAllTextAsync(localFile, "data");
            await service.UploadAsync(localFile, "remote");
            File.Delete(localFile);

            client.Verify(c => c.Connect(), Times.Once);
            client.Verify(c => c.Upload(It.IsAny<System.IO.Stream>(), "remote"), Times.Once);
            client.Verify(c => c.Disconnect(), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
