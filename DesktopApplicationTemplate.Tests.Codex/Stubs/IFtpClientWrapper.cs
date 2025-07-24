using System.Threading;
using System.Threading.Tasks;
using FluentFTP;

namespace DesktopApplicationTemplate.UI.Services
{
    public interface IFtpClientWrapper
    {
        Task Connect(CancellationToken token);
        Task<FtpStatus> UploadFile(string localPath, string remotePath, FtpRemoteExists existsMode, bool createRemoteDir, FtpVerify verify, IProgress<FtpProgress>? progress, CancellationToken token);
        Task Disconnect(CancellationToken token);
        string Host { get; }
        int Port { get; }
    }
}
