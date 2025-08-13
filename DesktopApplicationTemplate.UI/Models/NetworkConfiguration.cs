using System;

namespace DesktopApplicationTemplate.UI.Models
{
    public class NetworkConfiguration
    {
        public string IpAddress { get; init; } = string.Empty;
        public string SubnetMask { get; init; } = string.Empty;
        public string Gateway { get; init; } = string.Empty;
        public string DnsPrimary { get; init; } = string.Empty;
        public string DnsSecondary { get; init; } = string.Empty;
    }
}
