using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;
using System;
using Moq;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Helpers;
using System.Threading;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.Tests
{
    public class FtpServiceViewModelTests
    {
        private class StubFileDialogService : IFileDialogService
        {
            public string? OpenFile() => "stub.txt";
            public string? SelectFolder() => null;
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void BrowseCommand_InitialPathEmpty_DoesNotThrow()
        {
            var logger = new Mock<ILoggingService>();
            var helper = new SaveConfirmationHelper(logger.Object);
            var vm = new FtpServiceViewModel(helper, new StubFileDialogService());
            vm.BrowseCommand.Execute(null);
            Assert.Equal("stub.txt", vm.LocalPath);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public async Task TransferAsync_UsesProvidedService()
        {
            var mock = new Mock<IFtpService>();
            var logger = new Mock<ILoggingService>();
            var helper = new SaveConfirmationHelper(logger.Object);
            var vm = new FtpServiceViewModel(helper) { Service = mock.Object, Logger = logger.Object };
            vm.LocalPath = "local";
            vm.RemotePath = "remote";

            await vm.TransferAsync();

            mock.Verify(s => s.UploadAsync("local", "remote", It.IsAny<CancellationToken>()), Times.Once);
            logger.Verify(l => l.Log("Starting FTP transfer", LogLevel.Debug), Times.Once);
            logger.Verify(l => l.Log("Finished FTP transfer", LogLevel.Debug), Times.Once);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void SettingInvalidHost_AddsError()
        {
            var logger = new Mock<ILoggingService>();
            var helper = new SaveConfirmationHelper(logger.Object);
            var vm = new FtpServiceViewModel(helper) { Logger = logger.Object };
            vm.Host = "bad_host";

            Assert.True(vm.HasErrors);
            logger.Verify(l => l.Log("Invalid FTP host entered", LogLevel.Warning), Times.Once);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void SettingInvalidPort_AddsError()
        {
            var logger = new Mock<ILoggingService>();
            var helper = new SaveConfirmationHelper(logger.Object);
            var vm = new FtpServiceViewModel(helper) { Logger = logger.Object };
            vm.Port = "abc";

            Assert.True(vm.HasErrors);
            logger.Verify(l => l.Log("Invalid FTP port entered", LogLevel.Warning), Times.Once);

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        [TestCategory("CodexSafe")]
        [TestCategory("WindowsSafe")]
        public void PartialHost_WithTrailingDot_DoesNotError()
        {
            var helper = new SaveConfirmationHelper(new Mock<ILoggingService>().Object);
            var vm = new FtpServiceViewModel(helper);
            vm.Host = "192.168.";

            Assert.False(vm.HasErrors);

            ConsoleTestLogger.LogPass();
        }
    }
}
