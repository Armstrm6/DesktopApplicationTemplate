using System;

namespace DesktopApplicationTemplate.UI.Models
{
    /// <summary>
    /// Represents a single TCP message exchange including incoming and outgoing data.
    /// </summary>
    public class TcpMessageRow
    {
        /// <summary>Incoming message content.</summary>
        public string IncomingMessage { get; set; } = string.Empty;

        /// <summary>IP address of the sender.</summary>
        public string IncomingIp { get; set; } = string.Empty;

        /// <summary>Outgoing message content.</summary>
        public string OutgoingMessage { get; set; } = string.Empty;

        /// <summary>Service associated with the outgoing message.</summary>
        public string ConnectedService { get; set; } = string.Empty;

        /// <summary>Result of sending the outgoing message.</summary>
        public string Result { get; set; } = string.Empty;
    }
}
