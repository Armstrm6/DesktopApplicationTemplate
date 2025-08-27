using System;

namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Modes a TCP service can operate in.
    /// </summary>
    public enum TcpServiceMode
    {
        Listening,
        Sending,
        ReceiveAndSend
    }

    /// <summary>
    /// Configuration options for creating a TCP service.
    /// </summary>
    public class TcpServiceOptions
    {
        /// <summary>
        /// Remote host name or address.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// Port number used for the connection.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Indicates whether UDP should be used instead of TCP.
        /// </summary>
        public bool UseUdp { get; set; }

        /// <summary>
        /// Operating mode for the service.
        /// </summary>
        public TcpServiceMode Mode { get; set; } = TcpServiceMode.Listening;
    }
}
