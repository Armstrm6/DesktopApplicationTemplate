using FluentFTP;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace DesktopApplicationTemplate.UI.Services
{
    public class FtpService : IFtpService
    {
        private readonly AsyncFtpClient _client;
        private readonly ILoggingService? _logger;

        public FtpService(string host, int port, string user, string pass, ILoggingService? logger = null)
        {
            var credentials = new System.Net.NetworkCredential(user, pass);
            _client = new AsyncFtpClient(host, credentials)
            {
                Port = port
            };
            _logger = logger;
        }

        public FtpService(AsyncFtpClient client, ILoggingService? logger = null)
        {
            _client = client;
            _logger = logger;
        }

        public async Task UploadAsync(string localPath, string remotePath, CancellationToken token = default)
        {
            _logger?.Log($"Connecting to FTP {_client.Host}:{_client.Port}", LogLevel.Debug);
            try
            {
                await _client.Connect(token);
                _logger?.Log($"Uploading {localPath} -> {remotePath}", LogLevel.Debug);
                await _client.UploadFile(localPath, remotePath, FtpRemoteExists.Overwrite, true, FtpVerify.None, null, token);
                await _client.Disconnect(token);
                _logger?.Log("Upload finished", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _logger?.Log($"FTP upload failed: {ex.Message}", LogLevel.Error);
                throw;
            }
        }
    }
}
