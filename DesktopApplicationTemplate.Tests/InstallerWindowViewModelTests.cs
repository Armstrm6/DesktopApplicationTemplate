using System.IO;
using DesktopApplication.Installer.ViewModels;
using DesktopApplicationTemplate.UI.Services;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class InstallerWindowViewModelTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
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

    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void UninstallCommand_LogsAndRaisesEvent()
    {
        var temp = Path.Combine(Path.GetTempPath(), "installer_test" + Path.GetRandomFileName());
        Directory.CreateDirectory(temp);
        File.WriteAllText(Path.Combine(temp, "install_log.txt"), "installed");
        var logger = new Mock<ILoggingService>();
        var vm = new InstallerWindowViewModel(logger.Object) { InstallPath = temp };
        bool called = false;
        vm.UninstallRequested += _ => called = true;

        vm.UninstallCommand.Execute(null);

        Assert.True(called);
        logger.Verify(l => l.Log("Uninstall command triggered", LogLevel.Debug), Times.Once);
        Directory.Delete(temp, true);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void CheckUpdatesCommand_LogsAndRaisesEvent()
    {
        var logger = new Mock<ILoggingService>();
        var vm = new InstallerWindowViewModel(logger.Object);
        bool called = false;
        vm.CheckUpdatesRequested += () => called = true;

        vm.CheckUpdatesCommand.Execute(null);

        Assert.True(called);
        logger.Verify(l => l.Log("Check for updates command triggered", LogLevel.Debug), Times.Once);
        ConsoleTestLogger.LogPass();
    }
}
