using System.IO;
using Renci.SshNet;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Scp
{
    /// <summary>
    /// Wraps <see cref="ScpClient"/> to expose it through <see cref="IScpClient"/>.
    /// </summary>
    internal sealed class ScpClientWrapper : IScpClient
    {
        private readonly ScpClient _client;

        public ScpClientWrapper(ScpClient client)
        {
            _client = client;
        }

        public void Connect() => _client.Connect();

        public void Upload(Stream source, string path) => _client.Upload(source, path);

        public void Disconnect() => _client.Disconnect();

        public void Dispose() => _client.Dispose();
    }
}
