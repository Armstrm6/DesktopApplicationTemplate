using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;
using System;

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
    }
}
