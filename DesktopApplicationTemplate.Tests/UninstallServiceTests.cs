using System.IO;
using System.Threading.Tasks;
using DesktopApplication.Installer.Services;
using DesktopApplicationTemplate.UI.Services;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class UninstallServiceTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public async Task UninstallAsync_StopsServicesAndDeletesDirectory()
    {
        var temp = Path.Combine(Path.GetTempPath(), "uninstall_test" + Path.GetRandomFileName());
        Directory.CreateDirectory(temp);
        File.WriteAllText(Path.Combine(temp, "dummy.txt"), "data");
        var processManager = new Mock<IProcessManager>();
        processManager.Setup(p => p.GetProcessIdsByName(It.IsAny<string>())).Returns(new[] { 1, 2 });
        var logger = new Mock<ILoggingService>();
        var service = new UninstallService(processManager.Object, logger.Object);

        await service.UninstallAsync(temp);

        processManager.Verify(p => p.GetProcessIdsByName("DesktopApplicationTemplate.Service"), Times.Once);
        processManager.Verify(p => p.KillProcess(1), Times.Once);
        processManager.Verify(p => p.KillProcess(2), Times.Once);
        Assert.False(Directory.Exists(temp));
        logger.Verify(l => l.Log("Uninstallation started", LogLevel.Debug), Times.Once);
        logger.Verify(l => l.Log("Uninstallation completed", LogLevel.Debug), Times.Once);
        ConsoleTestLogger.LogPass();
    }
}
