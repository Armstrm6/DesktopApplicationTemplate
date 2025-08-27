using DesktopApplicationTemplate.Service.Services;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class ServiceRuleTests
{
    [Fact]
    public void ValidateRequired_ReturnsError_WhenNullOrWhitespace()
    {
        var rule = new ServiceRule();
        var error = rule.ValidateRequired("", "Name");
        Assert.NotNull(error);
    }

    [Fact]
    public void ValidateRequired_ReturnsNull_WhenValid()
    {
        var rule = new ServiceRule();
        var error = rule.ValidateRequired("value", "Name");
        Assert.Null(error);
    }

    [Fact]
    public void ValidatePort_ReturnsError_WhenOutOfRange()
    {
        var rule = new ServiceRule();
        var error = rule.ValidatePort(0);
        Assert.NotNull(error);
    }

    [Fact]
    public void ValidatePort_ReturnsNull_WhenValid()
    {
        var rule = new ServiceRule();
        var error = rule.ValidatePort(21);
        Assert.Null(error);
    }
}

