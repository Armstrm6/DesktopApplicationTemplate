using Renci.SshNet;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services
{
    public class ScpService
    {
        private readonly ScpClient _client;
        public ScpService(string host, int port, string user, string password)
        {
            _client = new ScpClient(host, port, user, password);
        }

        public async Task UploadAsync(string localPath, string remotePath)
        {
            await Task.Run(() =>
            {
                using var stream = System.IO.File.OpenRead(localPath);
                _client.Connect();
                _client.Upload(stream, remotePath);
                _client.Disconnect();
            });
        }
    }
}
