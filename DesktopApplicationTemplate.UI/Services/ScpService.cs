using Renci.SshNet;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services
{
    public class ScpService
    {
        private readonly IScpClient _client;
        private readonly ILoggingService? _logger;

        public ScpService(string host, int port, string user, string password, ILoggingService? logger = null)
        {
            _client = new ScpClientWrapper(new ScpClient(host, port, user, password));
            _logger = logger;
        }

        internal ScpService(IScpClient client, ILoggingService? logger = null)
        {
            _client = client;
            _logger = logger;
        }

        public async Task UploadAsync(string localPath, string remotePath)
        {
            await Task.Run(() =>
            {
                using var stream = System.IO.File.OpenRead(localPath);
                _logger?.Log($"Connecting to SCP {_client.ConnectionInfo.Host}:{_client.ConnectionInfo.Port}", LogLevel.Debug);
                _client.Connect();
                _logger?.Log($"Uploading {localPath} -> {remotePath}", LogLevel.Debug);
                _client.Upload(stream, remotePath);
                _client.Disconnect();
                _logger?.Log("Upload finished", LogLevel.Debug);
            });
        }
    }
}
