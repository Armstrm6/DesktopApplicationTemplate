using DesktopApplication.Installer.ViewModels;
using DesktopApplicationTemplate.UI.Services;
using Moq;
using Xunit;
using System.IO;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Tests;

public class ProgressWindowViewModelTests
{
    [Fact]
    public async Task StartAsync_LogsCompletion()
    {
        var temp = Path.Combine(Path.GetTempPath(), "installer_test" + Path.GetRandomFileName());
        Directory.CreateDirectory(temp);
        try
        {
            var logger = new Mock<ILoggingService>();
            var vm = new ProgressWindowViewModel(temp, false, false, logger.Object);
            await vm.StartAsync();

            logger.Verify(l => l.Log("Installation started", LogLevel.Debug), Times.Once);
            logger.Verify(l => l.Log("Installation completed", LogLevel.Debug), Times.Once);
            ConsoleTestLogger.LogPass();
        }
        finally
        {
            if (Directory.Exists(temp))
                Directory.Delete(temp, true);
        }
    }
}
