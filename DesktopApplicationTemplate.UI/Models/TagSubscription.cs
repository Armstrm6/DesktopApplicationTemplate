using MQTTnet.Protocol;

namespace DesktopApplicationTemplate.UI.Models
{
    /// <summary>
    /// Represents an MQTT topic subscription with its desired QoS level.
    /// </summary>
    public class TagSubscription
    {
        /// <summary>
        /// Gets or sets the topic to subscribe to.
        /// </summary>
        public string Topic { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the quality of service level for this subscription.
        /// </summary>
        public MqttQualityOfServiceLevel QoS { get; set; } = MqttQualityOfServiceLevel.AtMostOnce;
    }
}
