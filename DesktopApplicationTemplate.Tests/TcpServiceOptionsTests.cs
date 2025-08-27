using System.Collections.Generic;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class TcpServiceOptionsTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void Defaults_AreReasonable()
    {
        var options = new TcpServiceOptions();

        Assert.Equal(string.Empty, options.Host);
        Assert.Equal(0, options.Port);
        Assert.False(options.UseUdp);
        Assert.Equal(TcpServiceMode.Listening, options.Mode);
    }

    [Fact]
    [TestCategory("CodexSafe")]
    public void Bind_BindsValuesFromConfiguration()
    {
        var configDict = new Dictionary<string, string?>
        {
            ["TcpService:Host"] = "localhost",
            ["TcpService:Port"] = "9000",
            ["TcpService:UseUdp"] = "true",
            ["TcpService:Mode"] = "Sending"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        var services = new ServiceCollection();
        services.Configure<TcpServiceOptions>(configuration.GetSection("TcpService"));
        using var provider = services.BuildServiceProvider();

        var options = provider
            .GetRequiredService<IOptions<TcpServiceOptions>>().Value;

        Assert.Equal("localhost", options.Host);
        Assert.Equal(9000, options.Port);
        Assert.True(options.UseUdp);
        Assert.Equal(TcpServiceMode.Sending, options.Mode);
    }
}
