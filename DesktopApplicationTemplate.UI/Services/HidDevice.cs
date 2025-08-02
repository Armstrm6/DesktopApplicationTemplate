using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Basic HID device implementation that logs outgoing messages.
    /// </summary>
    public class HidDevice : IHidDevice
    {
        private readonly ILoggingService? _logger;

        public HidDevice(ILoggingService? logger = null)
        {
            _logger = logger;
        }

        public Task SendKeysAsync(string text, int keyDownTimeMs, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            _ = keyDownTimeMs;
            _logger?.Log($"HID device sending: {text}", LogLevel.Debug);
            return Task.CompletedTask;
        }
    }
}
