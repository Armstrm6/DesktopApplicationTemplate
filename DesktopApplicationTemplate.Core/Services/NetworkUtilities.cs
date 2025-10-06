using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using DesktopApplicationTemplate.Core.Models;

namespace DesktopApplicationTemplate.Core.Services
{
    public static class NetworkUtilities
    {
        public static int SubnetToCidr(string subnetMask)
        {
            if (string.IsNullOrWhiteSpace(subnetMask))
                return 0;
            var bytes = IPAddress.Parse(subnetMask).GetAddressBytes();
            int count = 0;
            foreach (var b in bytes)
            {
                count += Convert.ToString(b, 2).Count(c => c == '1');
            }
            return count;
        }

        public static string GetLocalIpAddress()
        {
            try
            {
                var addresses = Dns.GetHostAddresses(Dns.GetHostName())
                    .Where(a => a.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a))
                    .Select(a => a.ToString())
                    .ToList();

                if (addresses.Count > 0)
                {
                    return addresses[0];
                }
            }
            catch
            {
                // ignore lookup failures and fall back to loopback
            }

            return IPAddress.Loopback.ToString();
        }

        public static NetworkConfiguration GetLocalNetworkConfiguration()
        {
            var configuration = new NetworkConfiguration
            {
                IpAddress = GetLocalIpAddress()
            };

            try
            {
                foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (networkInterface.OperationalStatus != OperationalStatus.Up ||
                        networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    {
                        continue;
                    }

                    var properties = networkInterface.GetIPProperties();
                    var unicast = properties.UnicastAddresses
                        .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a.Address));

                    if (unicast is null)
                    {
                        continue;
                    }

                    configuration = new NetworkConfiguration
                    {
                        IpAddress = unicast.Address.ToString(),
                        SubnetMask = unicast.IPv4Mask?.ToString() ?? string.Empty,
                        Gateway = properties.GatewayAddresses.FirstOrDefault()?.Address.ToString() ?? string.Empty,
                        DnsPrimary = GetDnsAddress(properties.DnsAddresses, 0),
                        DnsSecondary = GetDnsAddress(properties.DnsAddresses, 1)
                    };

                    break;
                }
            }
            catch
            {
                // ignore failures and return whatever was discovered so far
            }

            return configuration;
        }

        private static string GetDnsAddress(IEnumerable<IPAddress> addresses, int index)
        {
            if (addresses is null)
            {
                return string.Empty;
            }

            var ipv4Addresses = addresses
                .Where(a => a.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a))
                .Select(a => a.ToString())
                .ToList();

            return index >= 0 && index < ipv4Addresses.Count ? ipv4Addresses[index] : string.Empty;
        }
    }
}
