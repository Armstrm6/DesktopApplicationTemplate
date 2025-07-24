namespace DesktopApplicationTemplate.UI.Services
{
    public interface IFtpService
    {
        System.Threading.Tasks.Task UploadAsync(string localPath, string remotePath, System.Threading.CancellationToken token = default);
    }
}
