namespace DesktopApplicationTemplate.Core.Services.Protocols.Csv;

/// <summary>
/// Tracks runtime state for CSV output generation.
/// </summary>
public class CsvServiceState
{
    /// <summary>
    /// Gets or sets the next index value to apply when generating file names.
    /// </summary>
    public int FileIndex { get; set; } = 0;

    /// <summary>
    /// Gets or sets a value indicating whether the header row has been written for the current file.
    /// </summary>
    public bool HeaderWritten { get; set; } = false;

    /// <summary>
    /// Gets or sets the current file name used for CSV output.
    /// </summary>
    public string? CurrentFileName { get; set; }

    /// <summary>
    /// Resets the state so the next write will create a fresh file and header.
    /// </summary>
    public void Reset()
    {
        HeaderWritten = false;
        CurrentFileName = null;
    }
}
