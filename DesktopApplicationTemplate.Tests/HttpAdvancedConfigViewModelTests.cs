using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class HttpAdvancedConfigViewModelTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void BackCommand_RaisesBackRequested()
    {
        var opts = new HttpServiceOptions();
        var vm = new HttpAdvancedConfigViewModel(opts);
        var raised = false;
        vm.BackRequested += () => raised = true;

        vm.BackCommand.Execute(null);

        Assert.True(raised);
        ConsoleTestLogger.LogPass();
    }
}
