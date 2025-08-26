using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class TcpEditServiceViewModelTests
{
    [Fact]
    public void SaveCommand_Raises_ServiceUpdated()
    {
        var options = new TcpServiceOptions { Host = "h", Port = 1, UseUdp = false, Mode = TcpServiceMode.Listening };
        var vm = new TcpEditServiceViewModel("svc", options);
        vm.Host = "new";
        vm.Port = 2;
        vm.Options.UseUdp = true;
        vm.Options.Mode = TcpServiceMode.Sending;
        string? name = null;
        TcpServiceOptions? received = null;
        vm.ServiceUpdated += (n, o) => { name = n; received = o; };

        vm.SaveCommand.Execute(null);

        Assert.Equal("svc", name);
        Assert.NotNull(received);
        Assert.Equal("new", received!.Host);
        Assert.Equal(2, received.Port);
        Assert.True(received.UseUdp);
        Assert.Equal(TcpServiceMode.Sending, received.Mode);
    }
}
