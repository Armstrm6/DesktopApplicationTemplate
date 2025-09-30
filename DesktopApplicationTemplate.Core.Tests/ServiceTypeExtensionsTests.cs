using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using FluentAssertions;
using Xunit;

namespace DesktopApplicationTemplate.Core.Tests;

public class ServiceTypeExtensionsTests
{
    [Theory]
    [InlineData(ServiceType.Mqtt, "MQ")]
    [InlineData(ServiceType.Http, "HT")]
    public void ToCode_ReturnsExpectedShortCode(ServiceType type, string expected)
    {
        type.ToCode().Should().Be(expected);
    }

    [Theory]
    [InlineData("TC", ServiceType.Tcp)]
    [InlineData("TCP", ServiceType.Tcp)]
    [InlineData("FTP Server", ServiceType.Ftp)]
    public void TryParse_HandlesCodesAndLegacyNames(string input, ServiceType expected)
    {
        ServiceTypeExtensions.TryParse(input, out var result).Should().BeTrue();
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ServiceType.Ftp, "FTP")]
    [InlineData(ServiceType.Csv, "CSV Creator")]
    public void ToLegacyString_ReturnsFriendlyName(ServiceType type, string expected)
    {
        type.ToLegacyString().Should().Be(expected);
    }

    [Theory]
    [InlineData(ServiceType.Tcp, ServiceDescriptorIds.Tcp)]
    [InlineData(ServiceType.Http, ServiceDescriptorIds.Http)]
    public void ToDescriptorId_ReturnsStableIdentifier(ServiceType type, string expected)
    {
        type.ToDescriptorId().Should().Be(expected);
    }
}

