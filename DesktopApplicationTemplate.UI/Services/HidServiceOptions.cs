namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Configuration options for HID services.
    /// </summary>
    public class HidServiceOptions
    {
        /// <summary>
        /// Message template sent by the HID device.
        /// </summary>
        public string MessageTemplate { get; set; } = string.Empty;

        /// <summary>
        /// USB protocol version.
        /// </summary>
        public string UsbProtocol { get; set; } = "2.0";

        /// <summary>
        /// Service name to forward messages to.
        /// </summary>
        public string AttachedService { get; set; } = string.Empty;

        /// <summary>
        /// Debounce time in milliseconds.
        /// </summary>
        public int DebounceTimeMs { get; set; }

        /// <summary>
        /// Key down duration in milliseconds.
        /// </summary>
        public int KeyDownTimeMs { get; set; }
    }
}
