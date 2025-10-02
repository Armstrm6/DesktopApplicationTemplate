namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Configuration options for Heartbeat services.
    /// </summary>
    public class HeartbeatServiceOptions
    {
        /// <summary>
        /// Base message to use when building the heartbeat payload.
        /// </summary>
        public string BaseMessage { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether a ping indicator should be appended.
        /// </summary>
        public bool IncludePing { get; set; }

        /// <summary>
        /// Indicates whether a status indicator should be appended.
        /// </summary>
        public bool IncludeStatus { get; set; }
    }
}
