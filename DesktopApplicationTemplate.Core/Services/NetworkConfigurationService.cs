using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Models;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.Core.Services
{
    public class NetworkConfigurationService : INetworkConfigurationService
    {
        private readonly IProcessRunner _processRunner;
        private readonly ILogger<NetworkConfigurationService>? _logger;

        public event EventHandler<NetworkConfiguration>? ConfigurationChanged;

        public NetworkConfigurationService(IProcessRunner processRunner, ILogger<NetworkConfigurationService>? logger = null)
        {
            _processRunner = processRunner;
            _logger = logger;
        }

        public Task<NetworkConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
        {
            var iface = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(n => n.Name.Equals("eth0", StringComparison.OrdinalIgnoreCase))
                ?? NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up);

            if (iface == null)
            {
                _logger?.LogWarning("No active network interface found");
                return Task.FromResult(new NetworkConfiguration());
            }

            var props = iface.GetIPProperties();
            var unicast = props.UnicastAddresses.FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);
            var gateway = props.GatewayAddresses.FirstOrDefault()?.Address.ToString() ?? string.Empty;
            var dns = props.DnsAddresses.Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                                        .Select(a => a.ToString()).ToList();
            var config = new NetworkConfiguration
            {
                IpAddress = unicast?.Address.ToString() ?? string.Empty,
                SubnetMask = unicast?.IPv4Mask?.ToString() ?? string.Empty,
                Gateway = gateway,
                DnsPrimary = dns.ElementAtOrDefault(0) ?? string.Empty,
                DnsSecondary = dns.ElementAtOrDefault(1) ?? string.Empty
            };
            _logger?.LogInformation("Retrieved network configuration for eth0: {IP}", config.IpAddress);
            return Task.FromResult(config);
        }

        public async Task ApplyConfigurationAsync(NetworkConfiguration configuration, CancellationToken cancellationToken = default)
        {
            _logger?.LogInformation("Applying network configuration: {IP}/{Subnet} GW {Gateway}", configuration.IpAddress, configuration.SubnetMask, configuration.Gateway);
            if (OperatingSystem.IsWindows())
            {
                await _processRunner.RunAsync("netsh", $"interface ip set address \"eth0\" static {configuration.IpAddress} {configuration.SubnetMask} {configuration.Gateway}", cancellationToken).ConfigureAwait(false);
                await _processRunner.RunAsync("netsh", $"interface ip set dns \"eth0\" static {configuration.DnsPrimary}", cancellationToken).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(configuration.DnsSecondary))
                {
                    await _processRunner.RunAsync("netsh", $"interface ip add dns \"eth0\" {configuration.DnsSecondary} index=2", cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                var prefix = NetworkUtilities.SubnetToCidr(configuration.SubnetMask);
                await _processRunner.RunAsync("ip", $"addr add {configuration.IpAddress}/{prefix} dev eth0", cancellationToken).ConfigureAwait(false);
                await _processRunner.RunAsync("ip", $"route add default via {configuration.Gateway} dev eth0", cancellationToken).ConfigureAwait(false);
                await _processRunner.RunAsync("sh", $"-c \"echo nameserver {configuration.DnsPrimary} > /etc/resolv.conf\"", cancellationToken).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(configuration.DnsSecondary))
                {
                    await _processRunner.RunAsync("sh", $"-c \"echo nameserver {configuration.DnsSecondary} >> /etc/resolv.conf\"", cancellationToken).ConfigureAwait(false);
                }
            }
            ConfigurationChanged?.Invoke(this, configuration);
        }

    }
}
