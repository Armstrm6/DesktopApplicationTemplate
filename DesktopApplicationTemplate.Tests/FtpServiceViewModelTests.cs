using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;
using System;
using Moq;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.Tests
{
    public class FtpServiceViewModelTests
    {
        [Fact]
        public void BrowseCommand_InitialPathEmpty_DoesNotThrow()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }
            var vm = new FtpServiceViewModel();
            vm.BrowseCommand.Execute(null);
            Assert.True(true); // command executed without exception

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        public async Task TransferAsync_UsesProvidedService()
        {
            var mock = new Mock<IFtpService>();
            var vm = new FtpServiceViewModel { Service = mock.Object };
            vm.LocalPath = "local";
            vm.RemotePath = "remote";

            await vm.TransferAsync();

            mock.Verify(s => s.UploadAsync("local", "remote"), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
