using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Csv;

/// <summary>
/// Represents the configuration for CSV log output.
/// </summary>
public class CsvConfiguration
{
    /// <summary>
    /// Gets or sets the file name pattern used when creating output files.
    /// </summary>
    public string FileNamePattern { get; set; } = "output_{timestamp}.csv";

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
[JsonConverter(typeof(CsvColumnDefinitionConverter))]
public class CsvColumnDefinition : INotifyPropertyChanged
{
    private string _name = "Column";
    private string _expression = string.Empty;
    private string? _format;

    /// <summary>
    /// Gets or sets the display name of the column.
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    /// Gets or sets the attribute expression resolved through the routing service.
    /// </summary>
    public string Expression
    {
        get => _expression;
        set => SetField(ref _expression, value);
    }

    /// <summary>
    /// Gets or sets an optional <see cref="string.Format(string, object?)"/> template applied to the resolved value.
    /// </summary>
    public string? Format
    {
        get => _format;
        set => SetField(ref _format, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
