using System;

namespace DesktopApplicationTemplate.UI.Models
{
    /// <summary>
    /// Options controlling MQTT connection settings.
    /// </summary>
    public class MqttServiceOptions
    {
        /// <summary>
        /// MQTT broker host name or IP address.
        /// </summary>
        public string Host { get; set; } = "127.0.0.1";

        /// <summary>
        /// MQTT broker port.
        /// </summary>
        public int Port { get; set; } = 1883;

        /// <summary>
        /// Client identifier used when connecting.
        /// </summary>
        public string ClientId { get; set; } = "client1";

        /// <summary>
        /// Username for authentication.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Password for authentication.
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}
