using FluentFTP;
using FluentFTP.Client.BaseClient; // Required for async methods
using System.Threading.Tasks;


namespace DesktopApplicationTemplate.UI.Services
{
    public class FtpService : IFtpService
    {
        private readonly FtpClient _client;
        private readonly ILoggingService? _logger;

        public FtpService(string host, int port, string user, string pass, ILoggingService? logger = null)
        {
            var credentials = new System.Net.NetworkCredential(user, pass);
            _client = new FtpClient(host, credentials)
            {
                Port = port
            };
            _logger = logger;
        }

        internal FtpService(FtpClient client, ILoggingService? logger = null)
        {
            _client = client;
            _logger = logger;
        }

        public async Task UploadAsync(string localPath, string remotePath)
        {
            _logger?.Log($"Connecting to FTP {_client.Host}:{_client.Port}", LogLevel.Debug);
            _client.Connect();
            _logger?.Log($"Uploading {localPath} -> {remotePath}", LogLevel.Debug);
            _client.UploadFile(localPath, remotePath, FtpRemoteExists.Overwrite);
            _client.Disconnect();
            _logger?.Log("Upload finished", LogLevel.Debug);
        }
    }
}
