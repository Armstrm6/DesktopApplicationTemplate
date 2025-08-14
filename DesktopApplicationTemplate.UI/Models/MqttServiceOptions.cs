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
        /// Unique identifier for this client.
        /// </summary>
        public string ClientId { get; set; } = "client1";

        /// <summary>
        /// Path to the Certificate Authority certificate for TLS validation.
        /// </summary>
        public string? TlsCaCertificatePath { get; set; }

        /// <summary>
        /// Path to the client certificate used for TLS authentication.
        /// </summary>
        public string? TlsClientCertificatePath { get; set; }

        /// <summary>
        /// Path to the client private key used for TLS authentication.
        /// </summary>
        public string? TlsClientKeyPath { get; set; }

        /// <summary>
        /// Seconds between keep-alive packets; 0 disables keep-alive.
        /// </summary>
        public int KeepAlive
        {
            get => _keepAlive;
            set
            {
                if (value < MinKeepAlive || value > MaxKeepAlive)
                    throw new ArgumentOutOfRangeException(nameof(value), $"KeepAlive must be between {MinKeepAlive} and {MaxKeepAlive} seconds.");
                _keepAlive = value;
            }
        }

        /// <summary>
        /// When true, the broker discards previous session state on connect.
        /// </summary>
        public bool CleanSession { get; set; } = true;

        /// <summary>
        /// Quality of Service level: 0 (At most once), 1 (At least once), or 2 (Exactly once).
        /// </summary>
        public int QoS
        {
            get => _qos;
            set
            {
                if (value < MinQoS || value > MaxQoS)
                    throw new ArgumentOutOfRangeException(nameof(value), $"QoS must be between {MinQoS} and {MaxQoS}.");
                _qos = value;
            }
        }

        /// <summary>
        /// When true, published messages are retained by the broker.
        /// </summary>
        public bool RetainFlag { get; set; }

        /// <summary>
        /// Last will message sent if the client disconnects unexpectedly.
        /// </summary>
        public string? WillMessage { get; set; }

        /// <summary>
        /// Seconds to wait before attempting to reconnect.
        /// </summary>
        public int ReconnectDelay
        {
            get => _reconnectDelay;
            set
            {
                if (value < MinReconnectDelay || value > MaxReconnectDelay)
                    throw new ArgumentOutOfRangeException(nameof(value), $"ReconnectDelay must be between {MinReconnectDelay} and {MaxReconnectDelay} seconds.");
                _reconnectDelay = value;
            }
        }
    }
}
