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
        var vm = new TcpCreateServiceViewModel(new ServiceRule());
        vm.ServiceName = "svc";
        vm.Host = "host";
        vm.Port = 1234;
        vm.Options.UseUdp = true;
        vm.Options.Mode = TcpServiceMode.ReceiveAndSend;
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
        var vm = new TcpCreateServiceViewModel(new ServiceRule());
        var cancelled = false;
        vm.EditCancelled += () => cancelled = true;

        vm.CancelCommand.Execute(null);

        Assert.True(cancelled);
    }

    [Fact]
    public void AdvancedConfigCommand_Raises_Event_WithOptions()
    {
        var vm = new TcpCreateServiceViewModel(new ServiceRule());
        vm.Host = "host";
        vm.Port = 123;
        vm.Options.UseUdp = true;
        vm.Options.Mode = TcpServiceMode.Sending;
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
        var vm = new TcpCreateServiceViewModel(new ServiceRule());
        vm.ServiceName = string.Empty;
        Assert.True(vm.HasErrors);
    }
}
