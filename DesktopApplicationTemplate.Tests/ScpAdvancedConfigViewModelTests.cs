using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using FluentAssertions;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class ScpAdvancedConfigViewModelTests
{
    [Fact]
    public void BackCommand_RaisesBackRequested()
    {
        var options = new ScpServiceOptions();
        var vm = new ScpAdvancedConfigViewModel(options);
        var raised = false;
        vm.BackRequested += () => raised = true;
        vm.BackCommand.Execute(null);
        raised.Should().BeTrue();
    }
}
