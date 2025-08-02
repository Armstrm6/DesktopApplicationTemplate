using System;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Coordinates sending messages to a HID device with optional debounce delays.
    /// </summary>
    public class HidService : IHidService
    {
        private readonly IHidDevice _device;
        private readonly IDelayService _delayService;
        private readonly ILoggingService? _logger;

        public HidService(IHidDevice device, IDelayService delayService, ILoggingService? logger = null)
        {
            _device = device;
            _delayService = delayService;
            _logger = logger;
        }

        public async Task SendAsync(string message, int debounceTimeMs, int keyDownTimeMs, CancellationToken token = default)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            _logger?.Log("HidService send start", LogLevel.Debug);
            await _delayService.DelayAsync(debounceTimeMs, token).ConfigureAwait(false);
            await _device.SendKeysAsync(message, keyDownTimeMs, token).ConfigureAwait(false);
            _logger?.Log("HidService send complete", LogLevel.Debug);
        }
    }
}
