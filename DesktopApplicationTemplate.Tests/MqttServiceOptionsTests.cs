using DesktopApplicationTemplate.UI.Services;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class MqttServiceOptionsTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void Defaults_AreReasonable()
    {
        var options = new MqttServiceOptions();

        Assert.Equal(string.Empty, options.Host);
        Assert.Equal(1883, options.Port);
        Assert.Equal(string.Empty, options.ClientId);
        Assert.False(options.UseTls);
        Assert.Equal(60, options.KeepAliveSeconds);
        Assert.True(options.CleanSession);
    }
}
