using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services
{
    public class HidService : IHidService
    {
        private readonly IHidDevice _device;
        private readonly ILoggingService? _logger;

        public HidService(IHidDevice? device = null, ILoggingService? logger = null)
        {
            _device = device ?? new KeyboardHidDevice();
            _logger = logger;
        }

        public async Task SendAsync(string message, int keyDownTimeMs, int debounceTimeMs, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(message))
                return;

            _logger?.Log("HidService send start", LogLevel.Debug);
            await _device.SendKeystrokesAsync(message, keyDownTimeMs, debounceTimeMs, token).ConfigureAwait(false);
            _logger?.Log("HidService send finished", LogLevel.Debug);
        }
    }
}
