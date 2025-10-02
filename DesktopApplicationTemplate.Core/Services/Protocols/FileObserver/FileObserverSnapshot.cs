namespace DesktopApplicationTemplate.Core.Services.Protocols.FileObserver;

/// <summary>
/// Represents the observed file content and associated metadata.
/// </summary>
public sealed class FileObserverSnapshot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileObserverSnapshot"/> class.
    /// </summary>
    public FileObserverSnapshot(string contents, string[] imageNames)
    {
        Contents = contents;
        ImageNames = imageNames;
    }

    /// <summary>
    /// Gets the file contents.
    /// </summary>
    public string Contents { get; }

    /// <summary>
    /// Gets the discovered image names.
    /// </summary>
    public string[] ImageNames { get; }
}
