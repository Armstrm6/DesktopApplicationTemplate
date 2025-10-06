using System.Collections.Generic;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Csv;

/// <summary>
/// Represents the configuration for CSV log output.
/// </summary>
public class CsvConfiguration
{
    /// <summary>
    /// Gets or sets the file name pattern used when creating output files.
    /// </summary>
    public string FileNamePattern { get; set; } = "output_{date}_{time}.csv";

    /// <summary>
    /// Gets or sets the directory where CSV files are written.
    /// </summary>
    public string? OutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ordered collection of CSV columns.
    /// </summary>
    public IList<CsvColumnDefinition> Columns { get; set; } = new List<CsvColumnDefinition>();
}

/// <summary>
/// Describes a single CSV column.
/// </summary>
public class CsvColumnDefinition
{
    /// <summary>
    /// Gets or sets the display name of the column.
    /// </summary>
    public string Name { get; set; } = "Column";

    /// <summary>
    /// Gets or sets the service associated with the column.
    /// </summary>
    public string Service { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the custom script used to populate the column.
    /// </summary>
    public string Script { get; set; } = string.Empty;
}
