using System;
using System.IO;
using System.Linq;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using MQTTnet.Protocol;
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

    [Fact]
    [TestCategory("CodexSafe")]
    public void CreateCommand_BuildsOptionsWithAdvancedSettings()
    {
        var tempCert = Path.GetTempFileName();
        File.WriteAllBytes(tempCert, new byte[] { 1, 2, 3 });
        var vm = new MqttCreateServiceViewModel();
        vm.ServiceName = "svc";
        vm.Host = "host";
        vm.Port = 1234;
        vm.ClientId = "client";
        vm.Username = "user";
        vm.Password = "pass";
        vm.UseTls = true;
        vm.ClientCertificatePath = tempCert;
        vm.WillTopic = "wt";
        vm.WillPayload = "wp";
        vm.WillQualityOfService = MqttQualityOfServiceLevel.AtLeastOnce;
        vm.WillRetain = true;
        vm.KeepAliveSeconds = 15;
        vm.CleanSession = false;
        vm.ReconnectDelaySeconds = 5;
        MqttServiceOptions? received = null;
        vm.ServiceCreated += (_, o) => received = o;

        vm.CreateCommand.Execute(null);

        File.Delete(tempCert);
        Assert.NotNull(received);
        Assert.Equal("host", received!.Host);
        Assert.Equal(1234, received.Port);
        Assert.Equal("client", received.ClientId);
        Assert.Equal("user", received.Username);
        Assert.Equal("pass", received.Password);
        Assert.True(received.UseTls);
        Assert.Equal("wt", received.WillTopic);
        Assert.Equal("wp", received.WillPayload);
        Assert.Equal(MqttQualityOfServiceLevel.AtLeastOnce, received.WillQualityOfService);
        Assert.True(received.WillRetain);
        Assert.Equal((ushort)15, received.KeepAliveSeconds);
        Assert.False(received.CleanSession);
        Assert.Equal(TimeSpan.FromSeconds(5), received.ReconnectDelay);
        Assert.NotNull(received.ClientCertificate);
        Assert.True(new byte[] { 1, 2, 3 }.SequenceEqual(received.ClientCertificate!));
    }

    [Fact]
    [TestCategory("CodexSafe")]
    public void CreateCommand_ConvertsBlankFieldsToNull()
    {
        var vm = new MqttCreateServiceViewModel();
        vm.ServiceName = "svc";
        vm.Host = "host";
        vm.Port = 1883;
        vm.ClientId = "client";
        vm.Username = "";
        vm.Password = " ";
        vm.WillTopic = "";
        vm.WillPayload = null;
        vm.ReconnectDelaySeconds = -1;
        MqttServiceOptions? received = null;
        vm.ServiceCreated += (_, o) => received = o;

        vm.CreateCommand.Execute(null);

        Assert.NotNull(received);
        Assert.Null(received!.Username);
        Assert.Null(received.Password);
        Assert.Null(received.WillTopic);
        Assert.Null(received.WillPayload);
        Assert.Null(received.ReconnectDelay);
    }
}
