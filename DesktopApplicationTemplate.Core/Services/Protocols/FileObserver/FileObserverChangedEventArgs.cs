using System;

namespace DesktopApplicationTemplate.Core.Services.Protocols.FileObserver;

/// <summary>
/// Provides data for <see cref="IFileObserverService.FileChanged"/>.
/// </summary>
public sealed class FileObserverChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileObserverChangedEventArgs"/> class.
    /// </summary>
    public FileObserverChangedEventArgs(string filePath, string contents)
    {
        FilePath = filePath;
        Contents = contents;
    }

    /// <summary>
    /// Gets the affected file path.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets the latest contents of the file.
    /// </summary>
    public string Contents { get; }
}
