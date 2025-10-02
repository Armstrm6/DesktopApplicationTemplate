using Renci.SshNet;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Scp
{
    /// <summary>
    /// Creates SCP client instances backed by <see cref="ScpClient"/>.
    /// </summary>
    public sealed class ScpClientFactory : IScpClientFactory
    {
        public IScpClient Create(string host, int port, string username, string password)
        {
            var client = new ScpClient(host, port, username, password);
            return new ScpClientWrapper(client);
        }
    }
}
