using System.Collections.Generic;

namespace DesktopApplicationTemplate.Models
{
    /// <summary>
    /// Configuration options for the MQTT service.
    /// </summary>
    public class MqttServiceOptions
    {
        /// <summary>
        /// Gets or sets the MQTT broker host name or IP address.
        /// </summary>
        public string Host { get; set; } = "127.0.0.1";

        /// <summary>
        /// Gets or sets the MQTT broker port.
        /// </summary>
        public int Port { get; set; } = 1883;

        /// <summary>
        /// Gets or sets the MQTT client identifier.
        /// </summary>
        public string ClientId { get; set; } = "client1";

        /// <summary>
        /// Gets or sets the username used for MQTT authentication.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password used for MQTT authentication.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the collection of topics to subscribe to on connection.
        /// </summary>
        public IList<string> Topics { get; set; } = new List<string>();
    }
}
