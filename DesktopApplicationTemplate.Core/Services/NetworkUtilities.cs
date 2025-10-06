using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

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
    }
}
