using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Service responsible for sending formatted HID messages to an attached device.
    /// </summary>
    public interface IHidService
    {
        /// <summary>
        /// Sends the specified message to the HID device.
        /// </summary>
        /// <param name="message">Message to transmit.</param>
        /// <param name="debounceTimeMs">Delay before sending, allowing hardware debounce.</param>
        /// <param name="keyDownTimeMs">Duration each key is held down.</param>
        /// <param name="token">Cancellation token.</param>
        Task SendAsync(string message, int debounceTimeMs, int keyDownTimeMs, CancellationToken token = default);
    }
}
