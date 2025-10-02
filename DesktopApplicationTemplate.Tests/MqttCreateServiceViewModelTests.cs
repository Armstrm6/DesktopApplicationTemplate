using System;
using System.IO;
using System.Linq;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Service.Services;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using MQTTnet.Protocol;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class MqttCreateServiceViewModelTests
{
    [Fact]
    public void SaveCommand_Raises_ServiceSaved()
    {
        IServiceRule rule = new ServiceRule();
        var vm = new MqttCreateServiceViewModel(rule);
        vm.ServiceName = "svc";
        vm.Host = "host";
        vm.Port = 1234;
        vm.ClientId = "client";
        vm.Username = "user";
        vm.Password = "pass";
        MqttServiceOptions? received = null;
        string? name = null;
        vm.ServiceSaved += (n, o) => { name = n; received = o; };

        vm.SaveCommand.Execute(null);

        Assert.Equal("svc", name);
        Assert.NotNull(received);
        Assert.Equal("host", received!.Host);
        Assert.Equal(1234, received.Port);
        Assert.Equal("client", received.ClientId);
    }

    [Fact]
    public void CancelCommand_Raises_EditCancelled()
    {
        IServiceRule rule = new ServiceRule();
        var vm = new MqttCreateServiceViewModel(rule);
        var cancelled = false;
        vm.EditCancelled += () => cancelled = true;

        vm.CancelCommand.Execute(null);

        Assert.True(cancelled);
    }

    [Fact]
    public void SaveCommand_BuildsOptionsWithAdvancedSettings()
    {
        var tempCert = Path.GetTempFileName();
        File.WriteAllBytes(tempCert, new byte[] { 1, 2, 3 });
        IServiceRule rule = new ServiceRule();
        var vm = new MqttCreateServiceViewModel(rule);
        vm.ServiceName = "svc";
        vm.Host = "host";
        vm.Port = 1234;
        vm.ClientId = "client";
        vm.Username = "user";
        vm.Password = "pass";
        vm.Options.ConnectionType = MqttConnectionType.MqttTls;
        vm.Options.WillTopic = "wt";
        vm.Options.WillPayload = "wp";
        vm.Options.WillQualityOfService = MqttQualityOfServiceLevel.AtLeastOnce;
        vm.Options.WillRetain = true;
        vm.Options.KeepAliveSeconds = 15;
        vm.Options.CleanSession = false;
        vm.Options.ReconnectDelay = TimeSpan.FromSeconds(5);
        vm.Options.ClientCertificate = File.ReadAllBytes(tempCert);
        MqttServiceOptions? received = null;
        vm.ServiceSaved += (_, o) => received = o;

        vm.SaveCommand.Execute(null);

        File.Delete(tempCert);
        Assert.NotNull(received);
        Assert.Equal("host", received!.Host);
        Assert.Equal(1234, received.Port);
        Assert.Equal("client", received.ClientId);
        Assert.Equal("user", received.Username);
        Assert.Equal("pass", received.Password);
        Assert.Equal(MqttConnectionType.MqttTls, received.ConnectionType);
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
    public void SaveCommand_ConvertsBlankFieldsToNull()
    {
        IServiceRule rule = new ServiceRule();
        var vm = new MqttCreateServiceViewModel(rule);
        vm.ServiceName = "svc";
        vm.Host = "host";
        vm.Port = 1883;
        vm.ClientId = "client";
        vm.Username = "";
        vm.Password = " ";
        vm.Options.WillTopic = "";
        vm.Options.WillPayload = null;
        vm.Options.ReconnectDelay = null;
        MqttServiceOptions? received = null;
        vm.ServiceSaved += (_, o) => received = o;

        vm.SaveCommand.Execute(null);

        Assert.NotNull(received);
        Assert.Null(received!.Username);
        Assert.Null(received.Password);
        Assert.Null(received.WillTopic);
        Assert.Null(received.WillPayload);
        Assert.Null(received.ReconnectDelay);
    }

    [Fact]
    public void SettingEmptyServiceName_AddsError()
    {
        IServiceRule rule = new ServiceRule();
        var vm = new MqttCreateServiceViewModel(rule);
        vm.ServiceName = string.Empty;
        Assert.True(vm.HasErrors);
    }
}
