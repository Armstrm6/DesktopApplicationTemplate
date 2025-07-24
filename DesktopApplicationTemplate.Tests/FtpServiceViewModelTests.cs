using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;
using System;
using Moq;
using DesktopApplicationTemplate.UI.Services;

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
            var vm = new FtpServiceViewModel(new StubFileDialogService());
            vm.BrowseCommand.Execute(null);
            Assert.Equal("stub.txt", vm.LocalPath);

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
