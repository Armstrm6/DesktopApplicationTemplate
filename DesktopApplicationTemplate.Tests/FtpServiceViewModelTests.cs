using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;
using System;
using Moq;
using DesktopApplicationTemplate.UI.Services;
using System.Threading;

namespace DesktopApplicationTemplate.Tests
{
    public class FtpServiceViewModelTests
    {
        private class StubFileDialogService : IFileDialogService
        {
            public string? OpenFile() => "stub.txt";
        }

        [Fact]
        [TestCategory("CodexSafe")]
        public void BrowseCommand_InitialPathEmpty_DoesNotThrow()
        {
            var vm = new FtpServiceViewModel(new StubFileDialogService());
            vm.BrowseCommand.Execute(null);
            Assert.Equal("stub.txt", vm.LocalPath);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        public async Task TransferAsync_UsesProvidedService()
        {
            var mock = new Mock<IFtpService>();
            var vm = new FtpServiceViewModel { Service = mock.Object };
            vm.LocalPath = "local";
            vm.RemotePath = "remote";

            await vm.TransferAsync();

            mock.Verify(s => s.UploadAsync("local", "remote", It.IsAny<CancellationToken>()), Times.Once);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        public void SettingInvalidHost_AddsError()
        {
            var logger = new Mock<ILoggingService>();
            var vm = new FtpServiceViewModel { Logger = logger.Object };
            vm.Host = "bad_host";

            Assert.True(vm.HasErrors);
            logger.Verify(l => l.Log("Invalid FTP host entered", LogLevel.Warning), Times.Once);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        public void SettingInvalidPort_AddsError()
        {
            var logger = new Mock<ILoggingService>();
            var vm = new FtpServiceViewModel { Logger = logger.Object };
            vm.Port = "abc";

            Assert.True(vm.HasErrors);
            logger.Verify(l => l.Log("Invalid FTP port entered", LogLevel.Warning), Times.Once);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        public void PartialHost_WithTrailingDot_DoesNotError()
        {
            var vm = new FtpServiceViewModel();
            vm.Host = "192.168.";

            Assert.False(vm.HasErrors);

            ConsoleTestLogger.LogPass();
        }
    }
}
