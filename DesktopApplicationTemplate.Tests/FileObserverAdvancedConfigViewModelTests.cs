using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using FluentAssertions;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class FileObserverAdvancedConfigViewModelTests
{
    [Fact]
    public void BackCommand_RaisesBackRequested()
    {
        var options = new FileObserverServiceOptions();
        var vm = new FileObserverAdvancedConfigViewModel(options);
        var raised = false;
        vm.BackRequested += () => raised = true;
        vm.BackCommand.Execute(null);
        raised.Should().BeTrue();
    }
}
