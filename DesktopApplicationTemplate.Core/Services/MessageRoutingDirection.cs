namespace DesktopApplicationTemplate.Core.Services
{
    /// <summary>
    /// Indicates whether a routed message represents inbound or outbound data.
    /// </summary>
    public enum MessageRoutingDirection
    {
        /// <summary>Represents the last inbound message for a service.</summary>
        Input,

        /// <summary>Represents the last outbound message for a service.</summary>
        Output
    }
}
