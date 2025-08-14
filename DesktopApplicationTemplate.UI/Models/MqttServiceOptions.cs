using System;
using System.Collections.Generic;

namespace DesktopApplicationTemplate.UI.Models
{
    /// <summary>
    /// Options for configuring MQTT connectivity.
    /// </summary>
    public class MqttServiceOptions
    {
        private const int MinPort = 1;
        private const int MaxPort = 65535;
        private const int MinKeepAlive = 0;
        private const int MaxKeepAlive = 65535;
        private const int MinQoS = 0;
        private const int MaxQoS = 2;
        private const int MinReconnectDelay = 0;
        private const int MaxReconnectDelay = 3600;

        private IList<int> _ports = new List<int> { 1883 };
        private int _keepAlive = 60;
        private int _qos = 0;
        private int _reconnectDelay = 5;

        /// <summary>
        /// Broker host name or IP address.
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// TCP ports to connect to. 1883 is standard MQTT; 8883 is MQTT over TLS.
        /// </summary>
        public IList<int> Ports
        {
            get => _ports;
            set
            {
                if (value == null || value.Count == 0)
                    throw new ArgumentException("At least one port is required.", nameof(value));

                foreach (var port in value)
                {
                    if (port < MinPort || port > MaxPort)
                        throw new ArgumentOutOfRangeException(nameof(value), $"Port {port} must be between {MinPort} and {MaxPort}.");
                }

                _ports = value;
            }
        }

        /// <summary>
        /// Connection protocol, e.g., 'mqtt' or 'mqtts'.
        /// </summary>
        public string Protocol { get; set; } = "mqtt";

        /// <summary>
        /// Username for broker authentication.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Password for broker authentication.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
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
