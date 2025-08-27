using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class CsvEditServiceViewModelTests
{
    [Fact]
    public void SaveCommand_Raises_ServiceUpdated()
    {
        var options = new CsvServiceOptions { OutputPath = "p", Delimiter = ";" };
        var vm = new CsvEditServiceViewModel("svc", options);
        CsvServiceOptions? received = null;
        string? name = null;
        vm.ServiceUpdated += (n, o) => { name = n; received = o; };

        vm.SaveCommand.Execute(null);

        Assert.Equal("svc", name);
        Assert.NotNull(received);
        Assert.Equal("p", received!.OutputPath);
        Assert.Equal(";", received.Delimiter);
    }
}
