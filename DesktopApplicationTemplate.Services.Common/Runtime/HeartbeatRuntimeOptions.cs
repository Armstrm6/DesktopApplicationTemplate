namespace DesktopApplicationTemplate.Services.Common.Runtime;

/// <summary>
/// Options controlling the heartbeat runtime workflow.
/// </summary>
public class HeartbeatRuntimeOptions
{
    /// <summary>
    /// Gets or sets the heartbeat message emitted on each interval.
    /// </summary>
    public string Message { get; set; } = "PING";

    /// <summary>
    /// Gets or sets the interval between heartbeat emissions in seconds.
    /// </summary>
    public int IntervalSeconds { get; set; } = 30;
}
