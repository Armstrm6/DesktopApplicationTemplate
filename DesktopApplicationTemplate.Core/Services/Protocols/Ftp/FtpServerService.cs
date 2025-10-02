using System;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols;
using FubarDev.FtpServer;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Ftp;

/// <summary>
/// Hosts an FTP server using the FubarDev FTP server library.
/// </summary>
public sealed class FtpServerService : IFtpServerService
{
    private readonly IFtpServerHost ftpServerHost;
    private readonly IProtocolLogger protocolLogger;
    private int connectedClients;

    /// <summary>
    /// Initializes a new instance of the <see cref="FtpServerService"/> class.
    /// </summary>
    /// <param name="ftpServerHost">The hosted FTP server instance.</param>
    /// <param name="protocolLogger">Logger that records protocol diagnostics.</param>
    /// <param name="name">Optional friendly name for the server instance.</param>
    public FtpServerService(IFtpServerHost ftpServerHost, IProtocolLogger protocolLogger, string? name = null)
    {
        this.ftpServerHost = ftpServerHost ?? throw new ArgumentNullException(nameof(ftpServerHost));
        this.protocolLogger = protocolLogger ?? throw new ArgumentNullException(nameof(protocolLogger));
        Name = string.IsNullOrWhiteSpace(name) ? "FTP Server" : name;
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public bool IsRunning { get; private set; }

    /// <inheritdoc />
    public event EventHandler<FtpTransferEventArgs>? FileReceived;

    /// <inheritdoc />
    public event EventHandler<FtpTransferEventArgs>? FileSent;

    /// <inheritdoc />
    public event EventHandler<FtpTransferProgressEventArgs>? TransferProgress;

    /// <inheritdoc />
    public event EventHandler<int>? ClientCountChanged;

    /// <inheritdoc />
    public int ConnectedClients => connectedClients;

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (IsRunning)
        {
            protocolLogger.LogWarning(this, "Start requested while the server is already running.");
            return;
        }

        protocolLogger.LogInformation(this, "Starting FTP server.");
        try
        {
            await ftpServerHost.StartAsync(cancellationToken).ConfigureAwait(false);
            IsRunning = true;
            protocolLogger.LogInformation(this, "FTP server started.");
        }
        catch (Exception ex)
        {
            protocolLogger.LogError(this, ex, "FTP server failed to start.");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!IsRunning)
        {
            protocolLogger.LogWarning(this, "Stop requested while the server is not running.");
            return;
        }

        protocolLogger.LogInformation(this, "Stopping FTP server.");
        try
        {
            await ftpServerHost.StopAsync(cancellationToken).ConfigureAwait(false);
            protocolLogger.LogInformation(this, "FTP server stopped.");
        }
        catch (Exception ex)
        {
            protocolLogger.LogError(this, ex, "FTP server failed to stop.");
            throw;
        }
        finally
        {
            IsRunning = false;
        }
    }

    /// <summary>
    /// Raises the <see cref="FileReceived"/> event.
    /// </summary>
    /// <param name="path">The received file path.</param>
    /// <param name="size">The size of the file in bytes.</param>
    public void RaiseFileReceived(string path, long size)
    {
        var args = new FtpTransferEventArgs(path, size, true);
        protocolLogger.LogInformation(this, $"Received {path} ({size} bytes)");
        FileReceived?.Invoke(this, args);
    }

    /// <summary>
    /// Raises the <see cref="FileSent"/> event.
    /// </summary>
    /// <param name="path">The sent file path.</param>
    /// <param name="size">The size of the file in bytes.</param>
    public void RaiseFileSent(string path, long size)
    {
        var args = new FtpTransferEventArgs(path, size, false);
        protocolLogger.LogInformation(this, $"Sent {path} ({size} bytes)");
        FileSent?.Invoke(this, args);
    }

    /// <summary>
    /// Raises the <see cref="TransferProgress"/> event.
    /// </summary>
    /// <param name="path">The file path being transferred.</param>
    /// <param name="size">Total size of the file.</param>
    /// <param name="transferred">Bytes transferred so far.</param>
    /// <param name="isUpload">Indicates whether the transfer is an upload.</param>
    public void RaiseTransferProgress(string path, long size, long transferred, bool isUpload)
    {
        var args = new FtpTransferProgressEventArgs(path, size, transferred, isUpload);
        protocolLogger.LogInformation(this, $"Progress {path} {transferred}/{size}");
        TransferProgress?.Invoke(this, args);
    }

    /// <summary>
    /// Raises the <see cref="ClientCountChanged"/> event.
    /// </summary>
    /// <param name="count">The current connected client count.</param>
    public void RaiseClientCountChanged(int count)
    {
        connectedClients = count;
        protocolLogger.LogInformation(this, $"Client count {count}");
        ClientCountChanged?.Invoke(this, count);
    }
}
