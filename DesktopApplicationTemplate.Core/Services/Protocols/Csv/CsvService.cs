using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Csv;

/// <summary>
/// Provides CSV logging capabilities that operate on plain configuration objects and output abstractions.
/// </summary>
public class CsvService : ICsvService
{
    /// <inheritdoc />
    public bool EnsureColumnsForService(CsvConfiguration configuration, string serviceName)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        if (IsCsvService(serviceName))
        {
            return false;
        }

        bool modified = false;
        if (!configuration.Columns.Any(c => string.Equals(c.Name, serviceName, StringComparison.Ordinal)))
        {
            configuration.Columns.Add(new CsvColumnDefinition
            {
                Name = serviceName,
                Service = serviceName
            });
            modified = true;
        }

        string sentColumn = $"{serviceName} Sent";
        if (!configuration.Columns.Any(c => string.Equals(c.Name, sentColumn, StringComparison.Ordinal)))
        {
            configuration.Columns.Add(new CsvColumnDefinition
            {
                Name = sentColumn,
                Service = serviceName
            });
            modified = true;
        }

        return modified;
    }

    /// <inheritdoc />
    public bool RemoveColumnsForService(CsvConfiguration configuration, CsvServiceState state, string serviceName)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        if (IsCsvService(serviceName))
        {
            return false;
        }

        string sentColumn = $"{serviceName} Sent";
        List<CsvColumnDefinition> removed = configuration.Columns
            .Where(c => string.Equals(c.Name, serviceName, StringComparison.Ordinal) ||
                        string.Equals(c.Name, sentColumn, StringComparison.Ordinal))
            .ToList();

        foreach (var column in removed)
        {
            configuration.Columns.Remove(column);
        }

        if (removed.Count > 0)
        {
            state.Reset();
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void RecordLog(CsvConfiguration configuration, CsvServiceState state, ICsvOutput output, string serviceName, string message)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        message ??= string.Empty;

        if (IsCsvService(serviceName))
        {
            return;
        }

        EnsureColumnsForService(configuration, serviceName);
        EnsureHeader(configuration, state, output);

        string[] values = configuration.Columns.Select(_ => string.Empty).ToArray();
        bool sent = message.Contains("Sending", StringComparison.OrdinalIgnoreCase) ||
                    message.Contains("Sent", StringComparison.OrdinalIgnoreCase);
        string columnName = sent ? $"{serviceName} Sent" : serviceName;

        int columnIndex = configuration.Columns
            .Select((column, index) => new { column, index })
            .FirstOrDefault(item => string.Equals(item.column.Name, columnName, StringComparison.Ordinal))?.index ?? -1;

        if (columnIndex >= 0)
        {
            values[columnIndex] = message.Replace(',', ' ');
        }

        AppendRow(configuration, state, output, values);
    }

    /// <inheritdoc />
    public void AppendRow(CsvConfiguration configuration, CsvServiceState state, ICsvOutput output, IEnumerable<string?> values)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(values);

        string filePath = BuildFileName(configuration, state, output);
        string line = string.Join(',', values.Select(value => value ?? string.Empty));
        output.AppendLine(filePath, line + Environment.NewLine);
    }

    private static void EnsureHeader(CsvConfiguration configuration, CsvServiceState state, ICsvOutput output)
    {
        if (state.HeaderWritten)
        {
            return;
        }

        string filePath = BuildFileName(configuration, state, output);
        if (!output.FileExists(filePath) || output.GetFileLength(filePath) == 0)
        {
            string header = string.Join(',', configuration.Columns.Select(column => column.Name));
            output.AppendLine(filePath, header + Environment.NewLine);
        }

        state.HeaderWritten = true;
    }

    private static string BuildFileName(CsvConfiguration configuration, CsvServiceState state, ICsvOutput output)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(output);

        if (!string.IsNullOrWhiteSpace(state.CurrentFilePath))
        {
            output.EnsureDirectoryForFile(state.CurrentFilePath);
            return state.CurrentFilePath!;
        }

        var pattern = configuration.FileNamePattern ?? string.Empty;
        var timestamp = state.FileTimestamp ?? DateTime.Now;
        state.FileTimestamp = timestamp;

        string fileName = pattern;
        if (fileName.Contains("{datetime}", StringComparison.Ordinal))
        {
            fileName = fileName.Replace("{datetime}", timestamp.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture));
        }

        if (fileName.Contains("{date}", StringComparison.Ordinal))
        {
            fileName = fileName.Replace("{date}", timestamp.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
        }

        if (fileName.Contains("{time}", StringComparison.Ordinal))
        {
            fileName = fileName.Replace("{time}", timestamp.ToString("HHmmss", CultureInfo.InvariantCulture));
        }

        if (fileName.Contains("{index}", StringComparison.Ordinal))
        {
            fileName = fileName.Replace("{index}", state.FileIndex.ToString(CultureInfo.InvariantCulture));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = $"output_{timestamp:yyyyMMdd_HHmmss}.csv";
        }

        string directory = configuration.OutputDirectory ?? string.Empty;
        string path = Path.Combine(directory, fileName);
        output.EnsureDirectoryForFile(path);
        state.CurrentFilePath = path;
        return path;
    }

    private static bool IsCsvService(string serviceName)
        => serviceName.Contains("CSV", StringComparison.OrdinalIgnoreCase);
}
