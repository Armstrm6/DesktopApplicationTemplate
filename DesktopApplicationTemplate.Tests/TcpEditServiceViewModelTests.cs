using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Service.Services;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class TcpEditServiceViewModelTests
{
    [Fact]
    public void SaveCommand_Raises_ServiceSaved()
    {
        var options = new TcpServiceOptions { Host = "h", Port = 1, UseUdp = false, Mode = TcpServiceMode.Listening };
        var vm = new TcpEditServiceViewModel(new ServiceRule());
        vm.Load("svc", options);
        vm.Host = "new";
        vm.Port = 2;
        options.UseUdp = true;
        options.Mode = TcpServiceMode.Sending;
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
    public void AdvancedConfigCommand_Raises_Event_WithUpdatedOptions()
    {
        var options = new TcpServiceOptions { Host = "h", Port = 1, UseUdp = false, Mode = TcpServiceMode.Listening };
        var vm = new TcpEditServiceViewModel(new ServiceRule());
        vm.Load("svc", options);
        vm.Host = "new";
        vm.Port = 2;
        options.UseUdp = true;
        options.Mode = TcpServiceMode.Sending;
        TcpServiceOptions? received = null;
        vm.AdvancedConfigRequested += o => received = o;

        vm.AdvancedConfigCommand.Execute(null);

        Assert.NotNull(received);
        Assert.Equal("new", received!.Host);
        Assert.Equal(2, received.Port);
        Assert.True(received.UseUdp);
        Assert.Equal(TcpServiceMode.Sending, received.Mode);
    }

    [Fact]
    public void SettingEmptyServiceName_AddsError()
    {
        var vm = new TcpEditServiceViewModel(new ServiceRule());
        vm.Load("svc", new TcpServiceOptions());
        vm.ServiceName = string.Empty;
        Assert.True(vm.HasErrors);
    }
}
