using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Csv.UI.ViewModels.Csv;

namespace DesktopApplicationTemplate.Services.Csv.UI.Services;

/// <summary>
/// Provides CSV helper functionality that integrates with the CSV viewer view model.
/// </summary>
public class CsvService : ICsvService
{
    private readonly CsvViewerViewModel _viewModel;
    private int _index;
    private bool _headerWritten;

    public CsvService(CsvViewerViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    private static bool IsCsvService(string serviceName) =>
        serviceName.Contains("CSV", System.StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public void EnsureColumnsForService(string serviceName)
    {
        if (IsCsvService(serviceName))
        {
            return;
        }

        if (!_viewModel.Configuration.Columns.Any(c => c.Name == serviceName))
        {
            _viewModel.Configuration.Columns.Add(new CsvColumnConfig { Name = serviceName, Service = serviceName });
        }

        var sentName = $"{serviceName} Sent";
        if (!_viewModel.Configuration.Columns.Any(c => c.Name == sentName))
        {
            _viewModel.Configuration.Columns.Add(new CsvColumnConfig { Name = sentName, Service = serviceName });
        }

        _viewModel.Save();
    }

    /// <inheritdoc />
    public void RemoveColumnsForService(string serviceName)
    {
        if (IsCsvService(serviceName))
        {
            return;
        }

        var sentName = $"{serviceName} Sent";
        var toRemove = _viewModel.Configuration.Columns
            .Where(c => c.Name == serviceName || c.Name == sentName)
            .ToList();

        foreach (var column in toRemove)
        {
            _viewModel.Configuration.Columns.Remove(column);
        }

        if (toRemove.Count > 0)
        {
            _viewModel.Save();
            _index++;
            _headerWritten = false;
        }
    }

    /// <inheritdoc />
    public void RecordLog(string serviceName, string message)
    {
        if (IsCsvService(serviceName))
        {
            return;
        }

        EnsureColumnsForService(serviceName);
        EnsureHeader();
        var columns = _viewModel.Configuration.Columns.Select(_ => string.Empty).ToArray();
        var sent = message.Contains("Sending", System.StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("Sent", System.StringComparison.OrdinalIgnoreCase);
        var column = sent ? $"{serviceName} Sent" : serviceName;
        var index = _viewModel.Configuration.Columns.ToList().FindIndex(c => c.Name == column);
        if (index >= 0)
        {
            columns[index] = message.Replace(',', ' ');
        }

        AppendRow(columns);
    }

    /// <inheritdoc />
    public void AppendRow(IEnumerable<string?> values)
    {
        var fileName = BuildFileName();
        var line = string.Join(',', values.Select(v => v ?? string.Empty));
        File.AppendAllText(fileName, line + System.Environment.NewLine, Encoding.UTF8);
    }

    private void EnsureHeader()
    {
        if (_headerWritten)
        {
            return;
        }

        var fileName = BuildFileName();
        if (!File.Exists(fileName) || new FileInfo(fileName).Length == 0)
        {
            var header = string.Join(',', _viewModel.Configuration.Columns.Select(c => c.Name));
            File.AppendAllText(fileName, header + System.Environment.NewLine, Encoding.UTF8);
        }

        _headerWritten = true;
    }

    private string BuildFileName()
    {
        var pattern = _viewModel.Configuration.FileNamePattern;
        var name = pattern.Replace("{index}", _index.ToString());
        if (pattern.Contains("{index}"))
        {
            _index++;
        }

        var directory = _viewModel.Configuration.OutputDirectory ?? string.Empty;
        var path = Path.Combine(directory, name);
        var folder = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return path;
    }
}
