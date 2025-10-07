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
/// Indicates whether the TCP service should behave as a client or server.
/// </summary>
public enum TcpConnectionRole
{
    Server,
    Client
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
    /// Remote host name or address used when the service is configured to send messages.
    /// </summary>
    public string DestinationHost { get; set; } = string.Empty;

    /// <summary>
    /// Remote port used when the service is configured to send messages.
    /// </summary>
    public int DestinationPort { get; set; }

    /// <summary>
    /// Gateway associated with the remote destination.
    /// </summary>
    public string DestinationGateway { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether UDP should be used instead of TCP.
    /// </summary>
    public bool UseUdp { get; set; }

    /// <summary>
    /// Subnet mask associated with the connection.
    /// </summary>
    public string SubnetMask { get; set; } = string.Empty;

    /// <summary>
    /// Primary DNS server used for resolving host names.
    /// </summary>
    public string PrimaryDns { get; set; } = string.Empty;

    /// <summary>
    /// Alternate DNS server used for resolving host names.
    /// </summary>
    public string AlternateDns { get; set; } = string.Empty;

    /// <summary>
    /// Operating mode for the service.
    /// </summary>
    public TcpServiceMode Mode { get; set; } = TcpServiceMode.Listening;

    /// <summary>
    /// Role of the TCP connection (client or server).
    /// </summary>
    public TcpConnectionRole ConnectionRole { get; set; } = TcpConnectionRole.Server;

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
