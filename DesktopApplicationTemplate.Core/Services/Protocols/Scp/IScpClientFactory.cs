namespace DesktopApplicationTemplate.Core.Services.Protocols.Scp
{
    /// <summary>
    /// Factory abstraction used to create <see cref="IScpClient"/> instances.
    /// </summary>
    public interface IScpClientFactory
    {
        /// <summary>
        /// Creates a new client configured for the supplied connection details.
        /// </summary>
        /// <param name="host">The remote host name or address.</param>
        /// <param name="port">The remote port.</param>
        /// <param name="username">The username used for authentication.</param>
        /// <param name="password">The password used for authentication.</param>
        /// <returns>A configured <see cref="IScpClient"/> instance.</returns>
        IScpClient Create(string host, int port, string username, string password);
    }
}
