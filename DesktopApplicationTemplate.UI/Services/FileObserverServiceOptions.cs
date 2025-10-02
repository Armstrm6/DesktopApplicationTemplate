namespace DesktopApplicationTemplate.UI.Services;

/// <summary>
/// Configuration options for File Observer services.
/// </summary>
public class FileObserverServiceOptions
{
    /// <summary>
    /// Path to observe for file changes.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated list of image names to send.
    /// </summary>
    public string ImageNames { get; set; } = string.Empty;

    /// <summary>
    /// Whether to send all images when triggered.
    /// </summary>
    public bool SendAllImages { get; set; }

    /// <summary>
    /// Whether to send only the first X images.
    /// </summary>
    public bool SendFirstX { get; set; }

    /// <summary>
    /// Number of images to send when <see cref="SendFirstX"/> is enabled.
    /// </summary>
    public int XCount { get; set; }

    /// <summary>
    /// Whether to send a TCP command.
    /// </summary>
    public bool SendTcpCommand { get; set; }

    /// <summary>
    /// TCP command string to send when <see cref="SendTcpCommand"/> is true.
    /// </summary>
    public string TcpCommand { get; set; } = string.Empty;
}
