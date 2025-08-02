using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Abstraction over a hardware device capable of sending HID keyboard input.
    /// </summary>
    public interface IHidDevice
    {
        /// <summary>
        /// Sends the specified text to the HID target.
        /// </summary>
        /// <param name="text">Text to send as keyboard input.</param>
        /// <param name="keyDownTimeMs">Duration in milliseconds each key is held down.</param>
        /// <param name="token">Cancellation token.</param>
        Task SendKeysAsync(string text, int keyDownTimeMs, CancellationToken token);
    }
}
