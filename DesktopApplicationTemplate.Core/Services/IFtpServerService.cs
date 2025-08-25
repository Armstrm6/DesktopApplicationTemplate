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
