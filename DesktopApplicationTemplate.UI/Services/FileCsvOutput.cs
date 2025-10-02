using System.IO;
using System.Text;
using DesktopApplicationTemplate.Core.Services.Protocols.Csv;

namespace DesktopApplicationTemplate.UI.Services;

/// <summary>
/// Provides file-based CSV output operations for the UI layer.
/// </summary>
public class FileCsvOutput : ICsvOutput
{
    /// <inheritdoc />
    public bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }

    /// <inheritdoc />
    public long GetFileLength(string filePath)
    {
        if (!FileExists(filePath))
        {
            return 0;
        }

        var info = new FileInfo(filePath);
        return info.Exists ? info.Length : 0;
    }

    /// <inheritdoc />
    public void AppendLine(string filePath, string content)
    {
        File.AppendAllText(filePath, content, Encoding.UTF8);
    }

    /// <inheritdoc />
    public void EnsureDirectoryForFile(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
