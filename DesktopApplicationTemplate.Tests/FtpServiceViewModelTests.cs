using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;
using System;
using Moq;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.Tests
{
    public class FtpServiceViewModelTests
    {
        private class StubFileDialogService : IFileDialogService
        {
            public string? OpenFile() => "stub.txt";
        }

        [Fact]
        public void BrowseCommand_InitialPathEmpty_DoesNotThrow()
        {
            var logger = new Mock<ILoggingService>();
            var vm = new FtpServiceViewModel(new StubFileDialogService()) { Logger = logger.Object };
            vm.BrowseCommand.Execute(null);
            Assert.Equal("stub.txt", vm.LocalPath);
            logger.Verify(l => l.Log("Browsing for file", LogLevel.Debug), Times.Once);
            logger.Verify(l => l.Log("File selected: stub.txt", LogLevel.Debug), Times.Once);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        public async Task TransferAsync_UsesProvidedService()
        {
            var mock = new Mock<IFtpService>();
            var logger = new Mock<ILoggingService>();
            var vm = new FtpServiceViewModel { Service = mock.Object, Logger = logger.Object };
            vm.LocalPath = "local";
            vm.RemotePath = "remote";

            await vm.TransferAsync();

            mock.Verify(s => s.UploadAsync("local", "remote"), Times.Once);
            logger.Verify(l => l.Log("Starting FTP upload", LogLevel.Debug), Times.Once);
            logger.Verify(l => l.Log("FTP upload complete", LogLevel.Debug), Times.Once);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
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
        public void PartialHost_WithTrailingDot_DoesNotError()
        {
            var vm = new FtpServiceViewModel();
            vm.Host = "192.168.";

            Assert.False(vm.HasErrors);

            ConsoleTestLogger.LogPass();
        }
    }
}
