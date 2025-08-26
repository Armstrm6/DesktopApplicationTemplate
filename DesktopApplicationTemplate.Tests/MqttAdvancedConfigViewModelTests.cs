using System.IO;
using System.Linq;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using MQTTnet.Protocol;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class MqttAdvancedConfigViewModelTests
{
    [Fact]
    public void SaveCommand_Raises_Saved_WithUpdatedOptions()
    {
        var tempCert = Path.GetTempFileName();
        File.WriteAllBytes(tempCert, new byte[] { 1, 2, 3 });
        var options = new MqttServiceOptions();
        var vm = new MqttAdvancedConfigViewModel(options);
        vm.UseTls = true;
        vm.ClientCertificatePath = tempCert;
        vm.WillTopic = "topic";
        vm.WillPayload = "payload";
        vm.WillQualityOfService = MqttQualityOfServiceLevel.AtLeastOnce;
        vm.WillRetain = true;
        vm.KeepAliveSeconds = 30;
        vm.CleanSession = false;
        vm.ReconnectDelaySeconds = 10;
        MqttServiceOptions? received = null;
        vm.Saved += o => received = o;

        vm.SaveCommand.Execute(null);

        File.Delete(tempCert);
        Assert.NotNull(received);
        Assert.True(received!.UseTls);
        Assert.Equal("topic", received.WillTopic);
        Assert.Equal("payload", received.WillPayload);
        Assert.Equal(MqttQualityOfServiceLevel.AtLeastOnce, received.WillQualityOfService);
        Assert.True(received.WillRetain);
        Assert.Equal((ushort)30, received.KeepAliveSeconds);
        Assert.False(received.CleanSession);
        Assert.Equal(System.TimeSpan.FromSeconds(10), received.ReconnectDelay);
        Assert.NotNull(received.ClientCertificate);
        Assert.True(new byte[] { 1, 2, 3 }.SequenceEqual(received.ClientCertificate!));
    }

    [Fact]
    public void BackCommand_Raises_BackRequested()
    {
        var vm = new MqttAdvancedConfigViewModel(new MqttServiceOptions());
        var called = false;
        vm.BackRequested += () => called = true;

        vm.BackCommand.Execute(null);

        Assert.True(called);
    }
}
