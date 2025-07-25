using DesktopApplication.Installer.ViewModels;
using DesktopApplicationTemplate.UI.Services;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class InstallerWindowViewModelTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    public void InstallCommand_LogsAndRaisesEvent()
    {
        var logger = new Mock<ILoggingService>();
        var vm = new InstallerWindowViewModel(logger.Object);
        bool called = false;
        vm.InstallRequested += (_, _, _) => called = true;

        vm.InstallCommand.Execute(null);

        Assert.True(called);
        logger.Verify(l => l.Log("Installer window initialized", LogLevel.Debug), Times.Once);
        logger.Verify(l => l.Log("Install command triggered", LogLevel.Debug), Times.Once);
        ConsoleTestLogger.LogPass();
    }
}
