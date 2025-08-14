using System;
using MQTTnet.Protocol;

namespace DesktopApplicationTemplate.UI.Services
{
    public enum MqttConnectionType
    {
        Tcp,
        WebSocket
    }

    public class MqttServiceOptions
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 1883;
        public string ClientId { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public MqttConnectionType ConnectionType { get; set; } = MqttConnectionType.Tcp;
        public bool UseTls { get; set; }
        public byte[]? ClientCertificate { get; set; }
        public string? WillTopic { get; set; }
        public string? WillPayload { get; set; }
        public MqttQualityOfServiceLevel WillQualityOfService { get; set; } = MqttQualityOfServiceLevel.AtMostOnce;
        public bool WillRetain { get; set; }
        public ushort KeepAliveSeconds { get; set; } = 60;
        public bool CleanSession { get; set; } = true;
        public TimeSpan? ReconnectDelay { get; set; }
    }
}
