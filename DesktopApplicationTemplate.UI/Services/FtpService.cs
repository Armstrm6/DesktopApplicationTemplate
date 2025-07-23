using FluentFTP;
using FluentFTP.Client.BaseClient; // Required for async methods
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
            _client.Connect();
            _client.UploadFile(localPath, remotePath, FtpRemoteExists.Overwrite);
            _client.Disconnect();
        }
    }
}
