using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services.Protocols;
using FluentFTP;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Ftp;

/// <summary>
/// Provides FTP client functionality backed by <see cref="FluentFTP"/>.
/// </summary>
public sealed class FtpService : IFtpClientService
{
    private readonly IAsyncFtpClient ftpClient;
    private readonly IProtocolLogger protocolLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FtpService"/> class using connection metadata.
    /// </summary>
    /// <param name="host">FTP server host name.</param>
    /// <param name="port">FTP server port.</param>
    /// <param name="username">Username for authentication.</param>
    /// <param name="password">Password for authentication.</param>
    /// <param name="protocolLogger">Logger that records protocol diagnostics.</param>
    public FtpService(string host, int port, string username, string password, IProtocolLogger protocolLogger, string? name = null)
        : this(CreateClient(host, port, username, password), protocolLogger, name)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FtpService"/> class using a prepared client.
    /// </summary>
    /// <param name="ftpClient">The FTP client implementation.</param>
    /// <param name="protocolLogger">Logger that records protocol diagnostics.</param>
    public FtpService(IAsyncFtpClient ftpClient, IProtocolLogger protocolLogger, string? name = null)
    {
        this.ftpClient = ftpClient ?? throw new ArgumentNullException(nameof(ftpClient));
        this.protocolLogger = protocolLogger ?? throw new ArgumentNullException(nameof(protocolLogger));
        Name = string.IsNullOrWhiteSpace(name)
            ? $"FTP Client ({this.ftpClient.Host}:{this.ftpClient.Port})"
            : name;
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public bool IsRunning { get; private set; }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (IsRunning)
        {
            protocolLogger.LogWarning(this, "Start requested while connection is already active.");
            return;
        }

        protocolLogger.LogInformation(this, $"Connecting to FTP {ftpClient.Host}:{ftpClient.Port}");
        try
        {
            await ftpClient.Connect(cancellationToken).ConfigureAwait(false);
            IsRunning = true;
            protocolLogger.LogInformation(this, "FTP connection established.");
        }
        catch (Exception ex)
        {
            protocolLogger.LogError(this, ex, "Failed to connect to the FTP server.");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!IsRunning)
        {
            protocolLogger.LogWarning(this, "Stop requested while connection is already closed.");
            return;
        }

        protocolLogger.LogInformation(this, "Disconnecting from the FTP server.");
        try
        {
            await ftpClient.Disconnect(cancellationToken).ConfigureAwait(false);
            protocolLogger.LogInformation(this, "FTP connection closed.");
        }
        finally
        {
            IsRunning = false;
        }
    }

    /// <inheritdoc />
    public async Task UploadAsync(string localPath, string remotePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(localPath))
            throw new ArgumentException("Local path must be provided.", nameof(localPath));
        if (string.IsNullOrWhiteSpace(remotePath))
            throw new ArgumentException("Remote path must be provided.", nameof(remotePath));

        var startedConnection = false;
        if (!IsRunning)
        {
            await StartAsync(cancellationToken).ConfigureAwait(false);
            startedConnection = true;
        }

        protocolLogger.LogInformation(this, $"Uploading {localPath} -> {remotePath}");
        try
        {
            await ftpClient.UploadFile(localPath, remotePath, FtpRemoteExists.Overwrite, true, FtpVerify.None, null, cancellationToken)
                .ConfigureAwait(false);
            protocolLogger.LogInformation(this, "Upload completed successfully.");
        }
        catch (Exception ex)
        {
            protocolLogger.LogError(this, ex, $"FTP upload failed for {localPath}.");
            throw;
        }
        finally
        {
            if (startedConnection)
            {
                await StopAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static IAsyncFtpClient CreateClient(string host, int port, string username, string password)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host must be provided.", nameof(host));
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username must be provided.", nameof(username));

        var credentials = new NetworkCredential(username, password);
        return new AsyncFtpClient(host, credentials)
        {
            Port = port
        };
    }
}
