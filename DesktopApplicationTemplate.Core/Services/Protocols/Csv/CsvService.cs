using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Csv;

/// <summary>
/// Provides CSV logging capabilities that operate on plain configuration objects and output abstractions.
/// </summary>
public class CsvService : ICsvService
{
    private readonly IMessageRoutingService _routingService;

    public CsvService(IMessageRoutingService routingService)
    {
        _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
    }

    /// <inheritdoc />
    public bool EnsureColumnsForService(CsvConfiguration configuration, string serviceName)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        return false;
    }

    /// <inheritdoc />
    public bool RemoveColumnsForService(CsvConfiguration configuration, CsvServiceState state, string serviceName)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        return false;
    }

    /// <inheritdoc />
    public void RecordLog(CsvConfiguration configuration, CsvServiceState state, ICsvOutput output, string serviceName, string message)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        var columns = configuration.Columns ?? Array.Empty<CsvColumnDefinition>();
        if (columns.Count == 0)
        {
            return;
        }

        EnsureHeader(configuration, state, output);

        var values = new string[columns.Count];
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            string resolved;
            try
            {
                resolved = CsvExpressionEvaluator.Evaluate(column.Expression, _routingService, serviceName);
            }
            catch
            {
                resolved = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(resolved) && string.IsNullOrWhiteSpace(column.Expression))
            {
                resolved = message ?? string.Empty;
            }

            resolved = CsvExpressionEvaluator.ApplyFormat(resolved, column.Format);
            values[i] = (resolved ?? string.Empty).Replace(',', ' ');
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

        EnsureHeader(configuration, state, output);

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

        if (configuration.Columns is null || configuration.Columns.Count == 0)
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

}
