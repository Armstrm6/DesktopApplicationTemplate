using Renci.SshNet;

namespace DesktopApplicationTemplate.UI.Services
{
    internal class ScpClientWrapper : IScpClient
    {
        private readonly ScpClient _client;
        public ScpClientWrapper(ScpClient client)
        {
            _client = client;
        }

        public ConnectionInfo ConnectionInfo => _client.ConnectionInfo;

        public void Connect() => _client.Connect();
        public void Upload(System.IO.Stream source, string path) => _client.Upload(source, path);
        public void Disconnect() => _client.Disconnect();
    }
}
