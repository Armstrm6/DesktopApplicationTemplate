using System;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Tcp;

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

        /// <summary>
        /// Local computer IP address used for listening services.
        /// </summary>
        public string ComputerIp { get; set; } = string.Empty;

        /// <summary>
        /// Local port displayed for listening services.
        /// </summary>
        public string ListeningPort { get; set; } = string.Empty;

        /// <summary>
        /// Destination server IP address for outbound connections.
        /// </summary>
        public string ServerIp { get; set; } = string.Empty;

        /// <summary>
        /// Gateway associated with the destination server.
        /// </summary>
        public string ServerGateway { get; set; } = string.Empty;

        /// <summary>
        /// Destination server port for outbound connections.
        /// </summary>
        public string ServerPort { get; set; } = string.Empty;

        /// <summary>
        /// Sample message used for testing script transformations.
        /// </summary>
        public string InputMessage { get; set; } = string.Empty;

    /// <summary>
    /// Script applied to <see cref="InputMessage"/> to produce an output.
    /// </summary>
    public string Script { get; set; } = string.Empty;

    /// <summary>
    /// Resulting message after executing <see cref="Script"/>.
    /// </summary>
    public string OutputMessage { get; set; } = string.Empty;

    /// <summary>
    /// Most recent test message entered in the TCP messages view.
    /// </summary>
    public string LastTestMessage { get; set; } = string.Empty;
}
