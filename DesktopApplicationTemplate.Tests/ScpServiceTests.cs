using DesktopApplicationTemplate.Core.Services;
using Moq;
using Renci.SshNet;
using System.Threading.Tasks;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class ScpServiceTests
    {
        [Fact]
        public async Task UploadAsync_InvokesClientOperations()
        {
            var client = new Mock<ScpClient>("host", 22, "u", "p");
            client.Setup(c => c.Connect());
            client.Setup(c => c.Upload(It.IsAny<System.IO.Stream>(), "remote"));
            client.Setup(c => c.Disconnect());

            var service = new ScpService(client.Object);
            await service.UploadAsync("local", "remote");

            client.Verify(c => c.Connect(), Times.Once);
            client.Verify(c => c.Upload(It.IsAny<System.IO.Stream>(), "remote"), Times.Once);
            client.Verify(c => c.Disconnect(), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
