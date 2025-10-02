using System;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using FubarDev.FtpServer;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.Services;

/// <summary>
/// Hosts an FTP server using the FubarDev FTP server library.
/// </summary>
public class FtpServerService : LoggedServiceBase<FtpServerService>, IFtpServerService
{
    private readonly IFtpServerHost _host;
    private int _connectedClients;

    public FtpServerService(IFtpServerHost host, ILogger<FtpServerService> logger) : base(logger)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    /// <inheritdoc />
    public event EventHandler<FtpTransferEventArgs>? FileReceived;

    /// <inheritdoc />
    public event EventHandler<FtpTransferEventArgs>? FileSent;

    /// <inheritdoc />
    public event EventHandler<FtpTransferProgressEventArgs>? TransferProgress;

    /// <inheritdoc />
    public event EventHandler<int>? ClientCountChanged;

    /// <inheritdoc />
    public int ConnectedClients => _connectedClients;

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(EventIds.Starting, "Starting FTP server");
        try
        {
            await _host.StartAsync(cancellationToken).ConfigureAwait(false);
            Logger.LogInformation(EventIds.Started, "FTP server started");
        }
        catch (Exception ex)
        {
            Logger.LogError(EventIds.StartFailed, ex, "FTP server failed to start");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(EventIds.Stopping, "Stopping FTP server");
        try
        {
            await _host.StopAsync(cancellationToken).ConfigureAwait(false);
            Logger.LogInformation(EventIds.Stopped, "FTP server stopped");
        }
        catch (Exception ex)
        {
            Logger.LogError(EventIds.StopFailed, ex, "FTP server failed to stop");
            throw;
        }
    }

    internal void RaiseFileReceived(string path, long size)
    {
        var args = new FtpTransferEventArgs(path, size, true);
        Logger.LogInformation(EventIds.FileReceived, "Received {Path} ({Size} bytes)", path, size);
        FileReceived?.Invoke(this, args);
    }

    internal void RaiseFileSent(string path, long size)
    {
        var args = new FtpTransferEventArgs(path, size, false);
        Logger.LogInformation(EventIds.FileSent, "Sent {Path} ({Size} bytes)", path, size);
        FileSent?.Invoke(this, args);
    }

    internal void RaiseTransferProgress(string path, long size, long transferred, bool isUpload)
    {
        var args = new FtpTransferProgressEventArgs(path, size, transferred, isUpload);
        Logger.LogInformation(EventIds.TransferProgress, "Progress {Path} {Transferred}/{Size}", path, transferred, size);
        TransferProgress?.Invoke(this, args);
    }

    internal void RaiseClientCountChanged(int count)
    {
        _connectedClients = count;
        Logger.LogInformation(EventIds.ClientCountChanged, "Client count {Count}", count);
        ClientCountChanged?.Invoke(this, count);
    }

    /// <summary>
    /// Event identifiers for <see cref="FtpServerService"/>.
    /// </summary>
    public static class EventIds
    {
        public static readonly EventId Starting = new(1000, nameof(Starting));
        public static readonly EventId Started = new(1001, nameof(Started));
        public static readonly EventId Stopping = new(1002, nameof(Stopping));
        public static readonly EventId Stopped = new(1003, nameof(Stopped));
        public static readonly EventId FileReceived = new(1004, nameof(FileReceived));
        public static readonly EventId FileSent = new(1005, nameof(FileSent));
        public static readonly EventId StartFailed = new(1006, nameof(StartFailed));
        public static readonly EventId StopFailed = new(1007, nameof(StopFailed));
        public static readonly EventId TransferProgress = new(1008, nameof(TransferProgress));
        public static readonly EventId ClientCountChanged = new(1009, nameof(ClientCountChanged));
    }
}
