using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Ftp;

/// <summary>
/// Service that hosts an FTP server instance.
/// </summary>
public interface IFtpServerService : IProtocolService
{
    /// <summary>
    /// Raised when a file is received by the server.
    /// </summary>
    event EventHandler<FtpTransferEventArgs> FileReceived;

    /// <summary>
    /// Raised when a file is sent by the server.
    /// </summary>
    event EventHandler<FtpTransferEventArgs> FileSent;

    /// <summary>
    /// Raised as transfer progress updates.
    /// </summary>
    event EventHandler<FtpTransferProgressEventArgs> TransferProgress;

    /// <summary>
    /// Raised when the number of connected clients changes.
    /// </summary>
    event EventHandler<int> ClientCountChanged;

    /// <summary>
    /// Gets the current number of connected clients.
    /// </summary>
    int ConnectedClients { get; }
}
