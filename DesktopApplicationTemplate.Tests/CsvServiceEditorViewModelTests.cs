using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Service.Services;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class CsvServiceEditorViewModelTests
{
    [Fact]
    public void SaveCommand_Raises_ServiceSaved()
    {
        IServiceRule rule = new ServiceRule();
        IServiceScreen<CsvServiceOptions> screen = new ServiceScreen<CsvServiceOptions>();
        var vm = new CsvServiceEditorViewModel(rule, screen);
        vm.ServiceName = "svc";
        vm.OutputPath = "path";
        string? name = null;
        CsvServiceOptions? received = null;
        vm.ServiceSaved += (n, o) => { name = n; received = o; };

        vm.SaveCommand.Execute(null);

        Assert.Equal("svc", name);
        Assert.NotNull(received);
        Assert.Equal("path", received!.OutputPath);
    }

    [Fact]
    public void Load_Sets_Existing_Values_ForEdit()
    {
        IServiceRule rule = new ServiceRule();
        IServiceScreen<CsvServiceOptions> screen = new ServiceScreen<CsvServiceOptions>();
        var vm = new CsvServiceEditorViewModel(rule, screen);
        var options = new CsvServiceOptions { OutputPath = "p" };

        vm.Load("svc", options);

        Assert.Equal("svc", vm.ServiceName);
        Assert.Equal("p", vm.OutputPath);
        Assert.Equal("Save", vm.SaveButtonText);
    }
}
