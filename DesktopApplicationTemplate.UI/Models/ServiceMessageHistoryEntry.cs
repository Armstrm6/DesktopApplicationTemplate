using System;

namespace DesktopApplicationTemplate.UI.Models
{
    /// <summary>
    /// Represents a persisted TCP message exchange for restoring UI state.
    /// </summary>
    public class ServiceMessageHistoryEntry
    {
        /// <summary>Incoming message content in its original form.</summary>
        public string IncomingMessage { get; set; } = string.Empty;

        /// <summary>Outgoing message content in its original form.</summary>
        public string OutgoingMessage { get; set; } = string.Empty;

        /// <summary>Associated destination for the outgoing payload.</summary>
        public string Destination { get; set; } = string.Empty;

        /// <summary>Timestamp when the exchange occurred.</summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
