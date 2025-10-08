using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Services;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class NetworkConfigurationServiceTests
    {
        [Fact]
        public async Task ApplyConfigurationAsync_InvokesProcessRunner()
        {
            var runner = new Mock<IProcessRunner>();
            var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<NetworkConfigurationService>();
            var service = new NetworkConfigurationService(runner.Object, logger);
            var config = new NetworkConfiguration
            {
                IpAddress = "1.1.1.1",
                SubnetMask = "255.255.255.0",
                Gateway = "1.1.1.254",
                DnsPrimary = "8.8.8.8"
            };

            await service.ApplyConfigurationAsync(config, CancellationToken.None);

            if (OperatingSystem.IsWindows())
            {
                runner.Verify(r => r.RunAsync("netsh", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            }
            else
            {
                runner.Verify(r => r.RunAsync("ip", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            }

            ConsoleTestLogger.LogPass();
        }
    }
}
