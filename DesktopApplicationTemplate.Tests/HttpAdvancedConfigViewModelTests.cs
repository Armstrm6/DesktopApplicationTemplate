using DesktopApplicationTemplate.Core.Services.Protocols.Http;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Http.Advanced;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class HttpAdvancedConfigViewModelTests
{
    [Fact]
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
