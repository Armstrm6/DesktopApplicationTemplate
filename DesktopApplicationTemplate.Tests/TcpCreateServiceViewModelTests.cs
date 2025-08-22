using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class TcpCreateServiceViewModelTests
{
    [Fact]
    public void CreateCommand_Raises_ServiceCreated()
    {
        var vm = new TcpCreateServiceViewModel();
        vm.ServiceName = "svc";
        vm.Host = "host";
        vm.Port = 1234;
        vm.UseUdp = true;
        vm.Mode = TcpServiceMode.ReceiveAndSend;
        TcpServiceOptions? received = null;
        string? name = null;
        vm.ServiceCreated += (n, o) => { name = n; received = o; };

        vm.CreateCommand.Execute(null);

        Assert.Equal("svc", name);
        Assert.NotNull(received);
        Assert.Equal("host", received!.Host);
        Assert.Equal(1234, received.Port);
        Assert.True(received.UseUdp);
        Assert.Equal(TcpServiceMode.ReceiveAndSend, received.Mode);
    }

    [Fact]
    public void CancelCommand_Raises_Cancelled()
    {
        var vm = new TcpCreateServiceViewModel();
        var cancelled = false;
        vm.Cancelled += () => cancelled = true;

        vm.CancelCommand.Execute(null);

        Assert.True(cancelled);
    }
}
