using System;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Services;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class NetworkConfigurationServiceTests
    {
        [Fact]
        public async Task ApplyConfigurationAsync_InvokesProcessRunner()
        {
            var runner = new Mock<IProcessRunner>();
            var logger = NullLogger<NetworkConfigurationService>.Instance;
            var service = new NetworkConfigurationService(runner.Object, logger);
            var interfaceName = OperatingSystem.IsWindows() ? "LAN 1" : "eth0";
            var config = new NetworkConfiguration
            {
                InterfaceName = interfaceName,
                IpAddress = "1.1.1.1",
                SubnetMask = "255.255.255.0",
                Gateway = "1.1.1.254",
                DnsPrimary = "8.8.8.8"
            };

            await service.ApplyConfigurationAsync(config, CancellationToken.None);

            if (OperatingSystem.IsWindows())
            {
                runner.Verify(r => r.RunAsync("netsh", It.Is<string>(args => args.Contains("name=\"LAN 1\"")), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            }
            else
            {
                runner.Verify(r => r.RunAsync("ip", It.Is<string>(args => args.Contains("dev eth0")), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            }

            ConsoleTestLogger.LogPass();
        }

        [Fact]
        public async Task ApplyConfigurationAsync_UsesInterfaceForDnsServers()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            var runner = new Mock<IProcessRunner>();
            var logger = NullLogger<NetworkConfigurationService>.Instance;
            var service = new NetworkConfigurationService(runner.Object, logger);
            var config = new NetworkConfiguration
            {
                InterfaceName = "LAN 1",
                IpAddress = "10.0.0.10",
                SubnetMask = "255.255.255.0",
                Gateway = "10.0.0.1",
                DnsPrimary = "8.8.8.8",
                DnsSecondary = "8.8.4.4"
            };

            await service.ApplyConfigurationAsync(config, CancellationToken.None);

            runner.Verify(r => r.RunAsync("netsh", It.Is<string>(args => args.StartsWith("interface ipv4 set address") && args.Contains("name=\"LAN 1\"")), It.IsAny<CancellationToken>()), Times.Once);
            runner.Verify(r => r.RunAsync("netsh", It.Is<string>(args => args.StartsWith("interface ipv4 set dnsservers") && args.Contains("name=\"LAN 1\"") && args.Contains("address=8.8.8.8")), It.IsAny<CancellationToken>()), Times.Once);
            runner.Verify(r => r.RunAsync("netsh", It.Is<string>(args => args.StartsWith("interface ipv4 add dnsservers") && args.Contains("name=\"LAN 1\"") && args.Contains("index=2") && args.Contains("8.8.4.4")), It.IsAny<CancellationToken>()), Times.Once);

            ConsoleTestLogger.LogPass();
        }
    }
}
