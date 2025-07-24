using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Core.Services
{
    public interface IFtpService
    {
        Task UploadAsync(string localPath, string remotePath);
    }
}
