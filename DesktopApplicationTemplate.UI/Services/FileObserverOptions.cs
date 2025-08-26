using System;

namespace DesktopApplicationTemplate.UI.Services;

/// <summary>
/// Configuration options for a file observer service.
/// </summary>
public class FileObserverOptions
{
    /// <summary>
    /// Path of the directory or file to observe.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Whether to include subdirectories when monitoring.
    /// </summary>
    public bool IncludeSubdirectories { get; set; }
}

