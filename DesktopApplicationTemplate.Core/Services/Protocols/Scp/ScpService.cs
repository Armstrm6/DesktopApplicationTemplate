using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Scp
{
    /// <summary>
    /// Provides SCP upload functionality backed by <see cref="IScpClient"/> implementations.
    /// </summary>
    public sealed class ScpService : IScpUploadService
    {
        private readonly IScpClientFactory _clientFactory;
        private readonly ILogger<ScpService> _logger;

        public ScpService(IScpClientFactory clientFactory, ILogger<ScpService> logger)
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task UploadAsync(
            string host,
            int port,
            string username,
            string password,
            string localPath,
            string remotePath,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("Host is required.", nameof(host));
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username is required.", nameof(username));
            }

            if (string.IsNullOrWhiteSpace(localPath))
            {
                throw new ArgumentException("Local path is required.", nameof(localPath));
            }

            if (string.IsNullOrWhiteSpace(remotePath))
            {
                throw new ArgumentException("Remote path is required.", nameof(remotePath));
            }

            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var client = _clientFactory.Create(host, port, username, password);
                using var stream = File.OpenRead(localPath);

                _logger.LogDebug("Connecting to SCP {Host}:{Port}", host, port);
                client.Connect();

                _logger.LogDebug("Uploading {LocalPath} -> {RemotePath}", localPath, remotePath);
                client.Upload(stream, remotePath);

                client.Disconnect();
                _logger.LogDebug("Upload finished");
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}
