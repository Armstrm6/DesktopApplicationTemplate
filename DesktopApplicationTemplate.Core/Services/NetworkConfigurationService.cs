using System;
using System.Collections.Generic;
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
            var iface = ResolveInterface(null);

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
                InterfaceName = iface.Name,
                IpAddress = unicast?.Address.ToString() ?? string.Empty,
                SubnetMask = unicast?.IPv4Mask?.ToString() ?? string.Empty,
                Gateway = gateway,
                DnsPrimary = dns.ElementAtOrDefault(0) ?? string.Empty,
                DnsSecondary = dns.ElementAtOrDefault(1) ?? string.Empty
            };
            _logger?.LogInformation("Retrieved network configuration for {InterfaceName}: {IP}", iface.Name, config.IpAddress);
            return Task.FromResult(config);
        }

        public async Task ApplyConfigurationAsync(NetworkConfiguration configuration, CancellationToken cancellationToken = default)
        {
            var interfaceName = GetInterfaceName(configuration.InterfaceName);
            if (string.IsNullOrWhiteSpace(interfaceName))
            {
                _logger?.LogWarning("No network interface available to apply configuration");
                return;
            }

            _logger?.LogInformation("Applying network configuration for {InterfaceName}: {IP}/{Subnet} GW {Gateway}", interfaceName, configuration.IpAddress, configuration.SubnetMask, configuration.Gateway);
            if (OperatingSystem.IsWindows())
            {
                var escapedName = EscapeInterfaceName(interfaceName);
                await _processRunner.RunAsync("netsh", $"interface ipv4 set address name=\"{escapedName}\" static {configuration.IpAddress} {configuration.SubnetMask} {configuration.Gateway}", cancellationToken).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(configuration.DnsPrimary))
                {
                    await _processRunner.RunAsync("netsh", $"interface ipv4 set dnsservers name=\"{escapedName}\" source=static address={configuration.DnsPrimary}", cancellationToken).ConfigureAwait(false);
                }
                if (!string.IsNullOrWhiteSpace(configuration.DnsSecondary))
                {
                    await _processRunner.RunAsync("netsh", $"interface ipv4 add dnsservers name=\"{escapedName}\" address={configuration.DnsSecondary} index=2", cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                var prefix = NetworkUtilities.SubnetToCidr(configuration.SubnetMask);
                await _processRunner.RunAsync("ip", $"addr add {configuration.IpAddress}/{prefix} dev {interfaceName}", cancellationToken).ConfigureAwait(false);
                await _processRunner.RunAsync("ip", $"route add default via {configuration.Gateway} dev {interfaceName}", cancellationToken).ConfigureAwait(false);
                await _processRunner.RunAsync("sh", $"-c \"echo nameserver {configuration.DnsPrimary} > /etc/resolv.conf\"", cancellationToken).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(configuration.DnsSecondary))
                {
                    await _processRunner.RunAsync("sh", $"-c \"echo nameserver {configuration.DnsSecondary} >> /etc/resolv.conf\"", cancellationToken).ConfigureAwait(false);
                }
            }
            var appliedConfiguration = new NetworkConfiguration
            {
                InterfaceName = interfaceName,
                IpAddress = configuration.IpAddress,
                SubnetMask = configuration.SubnetMask,
                Gateway = configuration.Gateway,
                DnsPrimary = configuration.DnsPrimary,
                DnsSecondary = configuration.DnsSecondary
            };
            ConfigurationChanged?.Invoke(this, appliedConfiguration);
        }

        public Task<IReadOnlyList<string>> GetAvailableInterfacesAsync(CancellationToken cancellationToken = default)
        {
            var names = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(n => n.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList()
                .AsReadOnly();
            return Task.FromResult((IReadOnlyList<string>)names);
        }

        private static string EscapeInterfaceName(string interfaceName) => interfaceName.Replace("\"", "\\\"");

        private static NetworkInterface? ResolveInterface(string? requestedInterface)
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            if (!string.IsNullOrWhiteSpace(requestedInterface))
            {
                var namedInterface = interfaces.FirstOrDefault(n => n.Name.Equals(requestedInterface, StringComparison.OrdinalIgnoreCase));
                if (namedInterface != null)
                {
                    return namedInterface;
                }
            }

            var preferred = interfaces.FirstOrDefault(n => n.Name.Equals("eth0", StringComparison.OrdinalIgnoreCase));
            if (preferred != null)
            {
                return preferred;
            }

            return interfaces.FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up && n.Supports(NetworkInterfaceComponent.IPv4));
        }

        private static string? GetInterfaceName(string? requestedInterface)
        {
            if (!string.IsNullOrWhiteSpace(requestedInterface))
            {
                return ResolveInterface(requestedInterface)?.Name ?? requestedInterface;
            }

            return ResolveInterface(null)?.Name;
        }

    }
}
