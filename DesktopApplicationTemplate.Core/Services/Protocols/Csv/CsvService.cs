using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        if (output is null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        var directory = configuration.OutputDirectory ?? string.Empty;
        if (string.IsNullOrWhiteSpace(state.CurrentFileName))
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            state.CurrentFileName = ResolveFileName(configuration.FileNamePattern, timestamp);
            state.HeaderWritten = false;
        }

        var path = Path.Combine(directory, state.CurrentFileName);
        output.EnsureDirectoryForFile(path);
        return path;
    }

    private static string ResolveFileName(string? pattern, string timestamp)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return $"output_{timestamp}.csv";
        }

        const string token = "{timestamp}";
        var index = pattern.IndexOf(token, StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            return string.Concat(pattern.AsSpan(0, index), timestamp, pattern.AsSpan(index + token.Length));
        }

        var fileName = Path.GetFileNameWithoutExtension(pattern);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = "output";
        }

        var extension = Path.GetExtension(pattern);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".csv";
        }

        return $"{fileName}_{timestamp}{extension}";
    }

    private static bool IsCsvService(string serviceName)
        => serviceName.Contains("CSV", StringComparison.OrdinalIgnoreCase);
}
