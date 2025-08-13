using FluentFTP;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.Service.Services
{
    public class FtpService : IFtpService
    {
        private readonly IAsyncFtpClient _client;
        private readonly ILoggingService? _logger;

        public FtpService(string host, int port, string user, string pass, ILoggingService? logger = null)
        {
            var credentials = new NetworkCredential(user, pass);
            _client = new AsyncFtpClient(host, credentials) { Port = port };
            _logger = logger;
        }

        public FtpService(IAsyncFtpClient client, ILoggingService? logger = null)
        {
            _client = client;
            _logger = logger;
        }

        public async Task UploadAsync(string localPath, string remotePath, CancellationToken token = default)
        {
            _logger?.Log($"Starting FTP upload {localPath} -> {remotePath}", LogLevel.Debug);
            _logger?.Log($"Connecting to FTP {_client.Host}:{_client.Port}", LogLevel.Debug);
            try
            {
                await _client.Connect(token);
                _logger?.Log($"Uploading {localPath} -> {remotePath}", LogLevel.Debug);
                await _client.UploadFile(localPath, remotePath, FtpRemoteExists.Overwrite, true, FtpVerify.None, null, token);
                await _client.Disconnect(token);
                _logger?.Log("Upload finished", LogLevel.Debug);
                _logger?.Log("FTP upload completed", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _logger?.Log($"FTP upload failed: {ex.Message}", LogLevel.Error);
                throw;
            }
        }
    }
}
