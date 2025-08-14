namespace DesktopApplicationTemplate.UI.Models
{
    /// <summary>
    /// Represents an MQTT endpoint and message pair.
    /// </summary>
    public class MqttEndpointMessage
    {
        /// <summary>
        /// Topic or endpoint to publish to.
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// Payload to publish.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
