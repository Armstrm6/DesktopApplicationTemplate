using System;
using DesktopApplicationTemplate.UI.Behaviors;
using FluentAssertions;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class TextBoxHintBehaviorTests
{
    [WindowsTheory]
    [InlineData("ServiceName", "Service Name")]
    [InlineData("TcpIp", "Tcp Ip")]
    public void GetFriendlyName_Splits_CamelCase(string input, string expected)
    {
        TextBoxHintBehavior.GetFriendlyName(input).Should().Be(expected);
    }

    [WindowsFact]
    public void GetFriendlyName_Throws_When_Null()
    {
        Action act = () => TextBoxHintBehavior.GetFriendlyName(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("propertyPath");
    }
}
