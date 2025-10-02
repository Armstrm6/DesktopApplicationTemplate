using System;
using System.IO;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Scp
{
    /// <summary>
    /// Represents a minimal abstraction over an SCP client implementation.
    /// </summary>
    public interface IScpClient : IDisposable
    {
        /// <summary>
        /// Opens the connection to the remote host.
        /// </summary>
        void Connect();

        /// <summary>
        /// Uploads a file stream to the specified remote path.
        /// </summary>
        /// <param name="source">The stream to upload.</param>
        /// <param name="path">The remote destination path.</param>
        void Upload(Stream source, string path);

        /// <summary>
        /// Closes the connection to the remote host.
        /// </summary>
        void Disconnect();
    }
}
