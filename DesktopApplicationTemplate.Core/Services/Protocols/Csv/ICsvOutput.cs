namespace DesktopApplicationTemplate.Core.Services.Protocols.Csv;

/// <summary>
/// Provides file system or stream operations required by the CSV service.
/// </summary>
public interface ICsvOutput
{
    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns><c>true</c> if the file exists; otherwise, <c>false</c>.</returns>
    bool FileExists(string filePath);

    /// <summary>
    /// Gets the length of the specified file in bytes.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file size in bytes.</returns>
    long GetFileLength(string filePath);

    /// <summary>
    /// Appends text to the specified file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="content">The content to append.</param>
    void AppendLine(string filePath, string content);

    /// <summary>
    /// Ensures the directory for the supplied file exists.
    /// </summary>
    /// <param name="filePath">The destination file path.</param>
    void EnsureDirectoryForFile(string filePath);
}
