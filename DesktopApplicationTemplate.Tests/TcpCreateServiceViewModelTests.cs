using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Service.Services;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
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
            Options = { UseUdp = true, Mode = TcpServiceMode.ReceiveAndSend }
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
    public void AdvancedConfigCommand_Raises_Event_WithOptions()
    {
        IServiceRule rule = new ServiceRule();
        var vm = new TcpCreateServiceViewModel(rule)
        {
            Host = "host",
            Port = 123,
            Options = { UseUdp = true, Mode = TcpServiceMode.Sending }
        };
        TcpServiceOptions? received = null;
        vm.AdvancedConfigRequested += o => received = o;

        vm.AdvancedConfigCommand.Execute(null);

        Assert.NotNull(received);
        Assert.Equal("host", received!.Host);
        Assert.Equal(123, received.Port);
        Assert.True(received.UseUdp);
        Assert.Equal(TcpServiceMode.Sending, received.Mode);
    }

    [Fact]
    public void SettingEmptyServiceName_AddsError()
    {
        IServiceRule rule = new ServiceRule();
        var vm = new TcpCreateServiceViewModel(rule) { ServiceName = string.Empty };
        Assert.True(vm.HasErrors);
    }
}
