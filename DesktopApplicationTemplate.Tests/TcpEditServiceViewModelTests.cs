using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Tcp.Edit;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class TcpEditServiceViewModelTests
{
    [Fact]
    public void SaveCommand_Raises_ServiceSaved()
    {
        var options = new TcpServiceOptions { Host = "h", Port = 1, UseUdp = false, Mode = TcpServiceMode.Listening };
        IServiceRule rule = new ServiceRule();
        var vm = new TcpEditServiceViewModel(rule);
        vm.Load("svc", options);
        vm.Host = "new";
        vm.Port = 2;
        vm.UseUdp = true;
        vm.Mode = TcpServiceMode.Sending;
        string? name = null;
        TcpServiceOptions? received = null;
        vm.ServiceSaved += (n, o) => { name = n; received = o; };

        vm.SaveCommand.Execute(null);

        Assert.Equal("svc", name);
        Assert.NotNull(received);
        Assert.Equal("new", received!.Host);
        Assert.Equal(2, received.Port);
        Assert.True(received.UseUdp);
        Assert.Equal(TcpServiceMode.Sending, received.Mode);
    }


    [Fact]
    public void SettingEmptyServiceName_AddsError()
    {
        IServiceRule rule = new ServiceRule();
        var vm = new TcpEditServiceViewModel(rule);
        vm.Load("svc", new TcpServiceOptions());
        vm.ServiceName = string.Empty;
        Assert.True(vm.HasErrors);
    }
}
