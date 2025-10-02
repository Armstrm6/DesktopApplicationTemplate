using System.Threading.Tasks;
using DesktopApplicationTemplate.UI.Models;

namespace DesktopApplicationTemplate.UI.Services
{
    public interface IStartupService
    {
        Task RunStartupChecksAsync();
        AppSettings GetSettings();
    }
}
