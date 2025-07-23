using FluentFTP;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services
{
    public class FtpService
    {
        private readonly FtpClient _client;
        public FtpService(string host, int port, string user, string pass)
        {
            var credentials = new System.Net.NetworkCredential(user, pass);
            _client = new FtpClient(host, credentials)
            {
                Port = port
            };

        }

        public async Task UploadAsync(string localPath, string remotePath)
        {
            await _client.ConnectAsync();
            await _client.UploadFileAsync(localPath, remotePath, FtpRemoteExists.Overwrite);
            await _client.DisconnectAsync();
        }
    }
}
