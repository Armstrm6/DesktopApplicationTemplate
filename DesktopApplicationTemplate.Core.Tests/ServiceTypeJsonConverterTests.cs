using System.Text.Json;
using DesktopApplicationTemplate.Core.Converters;
using DesktopApplicationTemplate.Models;
using FluentAssertions;
using Xunit;

namespace DesktopApplicationTemplate.Core.Tests;

public class ServiceTypeJsonConverterTests
{
    [Theory]
    [InlineData(ServiceType.Tcp, "\"TC\"")]
    [InlineData(ServiceType.Mqtt, "\"MQ\"")]
    public void Write_UsesShortCode(ServiceType type, string expected)
    {
        JsonSerializer.Serialize(type).Should().Be(expected);
    }

    [Theory]
    [InlineData("TC", ServiceType.Tcp)]
    [InlineData("TCP", ServiceType.Tcp)]
    [InlineData("FTP Server", ServiceType.Ftp)]
    [InlineData("MQ", ServiceType.Mqtt)]
    public void Read_ParsesCodesAndLegacyNames(string code, ServiceType expected)
    {
        var json = $"\"{code}\"";
        JsonSerializer.Deserialize<ServiceType>(json).Should().Be(expected);
    }
}
