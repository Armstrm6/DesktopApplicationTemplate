using DesktopApplicationTemplate.Core.Services;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class NetworkUtilitiesTests
{
    [Fact]
    public void SubnetToCidr_ConvertsMask()
    {
        var cidr = NetworkUtilities.SubnetToCidr("255.255.255.0");
        Assert.Equal(24, cidr);
    }
}
