using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Scp
{
    /// <summary>
    /// Provides operations for uploading files via SCP.
    /// </summary>
    public interface IScpUploadService
    {
        /// <summary>
        /// Uploads the specified file to the remote path using SCP.
        /// </summary>
        /// <param name="host">Remote host name or address.</param>
        /// <param name="port">Remote host port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <param name="localPath">The local file path to upload.</param>
        /// <param name="remotePath">The remote path destination.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        Task UploadAsync(
            string host,
            int port,
            string username,
            string password,
            string localPath,
            string remotePath,
            CancellationToken cancellationToken = default);
    }
}
