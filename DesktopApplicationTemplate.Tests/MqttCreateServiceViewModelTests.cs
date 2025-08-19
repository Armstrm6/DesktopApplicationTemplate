using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class MqttCreateServiceViewModelTests
{
    [Fact]
    public void CreateCommand_Raises_ServiceCreated()
    {
        var vm = new MqttCreateServiceViewModel();
        vm.ServiceName = "svc";
        vm.Host = "host";
        vm.Port = 1234;
        vm.ClientId = "client";
        MqttServiceOptions? received = null;
        string? name = null;
        vm.ServiceCreated += (n, o) => { name = n; received = o; };

        vm.CreateCommand.Execute(null);

        Assert.Equal("svc", name);
        Assert.NotNull(received);
        Assert.Equal("host", received!.Host);
        Assert.Equal(1234, received.Port);
        Assert.Equal("client", received.ClientId);
    }

    [Fact]
    public void CancelCommand_Raises_Cancelled()
    {
        var vm = new MqttCreateServiceViewModel();
        var cancelled = false;
        vm.Cancelled += () => cancelled = true;

        vm.CancelCommand.Execute(null);

        Assert.True(cancelled);
    }
}
