using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services
{
    public interface IHidService
    {
        Task SendAsync(string message, int keyDownTimeMs, int debounceTimeMs, CancellationToken token = default);
    }
}
