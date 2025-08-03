using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services
{
    public interface IHidDevice
    {
        Task SendKeystrokesAsync(string text, int keyDownTimeMs, int debounceTimeMs, CancellationToken token = default);
    }
}
