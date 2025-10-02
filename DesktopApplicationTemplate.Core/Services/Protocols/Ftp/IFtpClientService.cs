using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services.Protocols;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Ftp;

/// <summary>
/// Defines FTP client operations that can be hosted inside the protocol runtime.
/// </summary>
public interface IFtpClientService : IProtocolService
{
    /// <summary>
    /// Uploads a local file to the configured FTP endpoint.
    /// </summary>
    /// <param name="localPath">The local file path to upload.</param>
    /// <param name="remotePath">The destination path on the remote FTP server.</param>
    /// <param name="cancellationToken">Token used to cancel the upload operation.</param>
    Task UploadAsync(string localPath, string remotePath, CancellationToken cancellationToken = default);
}
