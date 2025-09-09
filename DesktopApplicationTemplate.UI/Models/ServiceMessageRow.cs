using System;

namespace DesktopApplicationTemplate.UI.Models
{
    /// <summary>
    /// Represents a single service message exchange.
    /// </summary>
    public class ServiceMessageRow
    {
        /// <summary>Incoming message content.</summary>
        public string IncomingMessage { get; set; } = string.Empty;

        /// <summary>Outgoing message content.</summary>
        public string OutgoingMessage { get; set; } = string.Empty;

        /// <summary>Destination service endpoint or route.</summary>
        public string Destination { get; set; } = string.Empty;

        /// <summary>Timestamp of the message exchange.</summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
