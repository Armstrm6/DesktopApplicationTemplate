using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services
{
    public interface IFtpService
    {
        Task UploadAsync(string localPath, string remotePath);
    }
}
