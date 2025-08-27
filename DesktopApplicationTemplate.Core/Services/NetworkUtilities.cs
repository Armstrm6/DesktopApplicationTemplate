using System;
using System.Net;
using System.Linq;

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
    }
}
