using FluentFTP;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services
{
    public class AsyncFtpClientAdapter : IFtpClientWrapper
    {
        private readonly AsyncFtpClient _inner;
        public AsyncFtpClientAdapter(AsyncFtpClient inner)
        {
            _inner = inner;
        }
        public string Host => _inner.Host;
        public int Port => _inner.Port;
        public Task Connect(CancellationToken token) => _inner.Connect(token);
        public Task<FtpStatus> UploadFile(string localPath, string remotePath, FtpRemoteExists existsMode, bool createRemoteDir, FtpVerify verify, IProgress<FtpProgress>? progress, CancellationToken token)
            => _inner.UploadFile(localPath, remotePath, existsMode, createRemoteDir, verify, progress, token);
        public Task Disconnect(CancellationToken token) => _inner.Disconnect(token);
    }
}
