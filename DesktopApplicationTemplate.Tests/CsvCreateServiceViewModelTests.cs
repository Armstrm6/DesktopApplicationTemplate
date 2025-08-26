using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class CsvCreateServiceViewModelTests
{
    [Fact]
    public void CreateCommand_Raises_ServiceCreated()
    {
        var vm = new CsvCreateServiceViewModel();
        vm.ServiceName = "svc";
        vm.OutputPath = "path";
        CsvServiceOptions? received = null;
        string? name = null;
        vm.ServiceCreated += (n, o) => { name = n; received = o; };

        vm.CreateCommand.Execute(null);

        Assert.Equal("svc", name);
        Assert.NotNull(received);
        Assert.Equal("path", received!.OutputPath);
    }

    [Fact]
    public void CancelCommand_Raises_Cancelled()
    {
        var vm = new CsvCreateServiceViewModel();
        var cancelled = false;
        vm.Cancelled += () => cancelled = true;

        vm.CancelCommand.Execute(null);

        Assert.True(cancelled);
    }

    [Fact]
    public void AdvancedConfigCommand_Raises_Event_WithOptions()
    {
        var vm = new CsvCreateServiceViewModel();
        vm.OutputPath = "p";
        CsvServiceOptions? received = null;
        vm.AdvancedConfigRequested += o => received = o;

        vm.AdvancedConfigCommand.Execute(null);

        Assert.NotNull(received);
        Assert.Equal("p", received!.OutputPath);
    }
}
