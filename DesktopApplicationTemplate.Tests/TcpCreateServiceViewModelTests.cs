using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Tcp;
using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Tcp.Create;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class TcpCreateServiceViewModelTests
{
    [Fact]
    public void SaveCommand_Raises_ServiceSaved()
    {
        IServiceRule rule = new ServiceRule();
        var vm = new TcpCreateServiceViewModel(rule)
        {
            ServiceName = "svc",
            Host = "host",
            Port = 1234,
            UseUdp = true,
            Mode = TcpServiceMode.ReceiveAndSend
        };
        TcpServiceOptions? received = null;
        string? name = null;
        vm.ServiceSaved += (n, o) => { name = n; received = o; };

        vm.SaveCommand.Execute(null);

        Assert.Equal("svc", name);
        Assert.NotNull(received);
        Assert.Equal("host", received!.Host);
        Assert.Equal(1234, received.Port);
        Assert.True(received.UseUdp);
        Assert.Equal(TcpServiceMode.ReceiveAndSend, received.Mode);
    }

    [Fact]
    public void CancelCommand_Raises_EditCancelled()
    {
        IServiceRule rule = new ServiceRule();
        var vm = new TcpCreateServiceViewModel(rule);
        var cancelled = false;
        vm.EditCancelled += () => cancelled = true;

        vm.CancelCommand.Execute(null);

        Assert.True(cancelled);
    }


    [Fact]
    public void SettingEmptyServiceName_AddsError()
    {
        IServiceRule rule = new ServiceRule();
        var vm = new TcpCreateServiceViewModel(rule) { ServiceName = string.Empty };
        Assert.True(vm.HasErrors);
    }
}
