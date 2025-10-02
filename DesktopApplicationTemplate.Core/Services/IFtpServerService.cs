using System;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Core.Services
{
    /// <summary>
    /// Service that hosts an FTP server.
    /// </summary>
    public interface IFtpServerService
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

        /// <summary>
        /// Starts the FTP server.
        /// </summary>
        /// <param name="cancellationToken">Token to observe cancellation requests.</param>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops the FTP server.
        /// </summary>
        /// <param name="cancellationToken">Token to observe cancellation requests.</param>
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}
