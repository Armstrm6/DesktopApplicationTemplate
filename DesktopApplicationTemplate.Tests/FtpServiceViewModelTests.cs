using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class FtpServiceViewModelTests
    {
        [Fact]
        public void BrowseCommand_InitialPathEmpty_DoesNotThrow()
        {
            var vm = new FtpServiceViewModel();
            vm.BrowseCommand.Execute(null);
            Assert.True(true); // command executed without exception
        }
    }
}
