using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class FileObserverAdvancedConfigViewModelTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void BackCommand_RaisesBackRequested()
    {
        var opts = new FileObserverOptions();
        var vm = new FileObserverAdvancedConfigViewModel(opts);
        var raised = false;
        vm.BackRequested += () => raised = true;

        vm.BackCommand.Execute(null);

        Assert.True(raised);
        ConsoleTestLogger.LogPass();
    }
}
