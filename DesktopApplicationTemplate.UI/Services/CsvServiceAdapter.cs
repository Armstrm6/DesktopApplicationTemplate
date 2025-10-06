using System;
using System.Collections.Generic;
using System.IO;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Csv;
using DesktopApplicationTemplate.UI.ViewModels.Csv;

using ProtocolCsvService = DesktopApplicationTemplate.Core.Services.Protocols.Csv.ICsvService;

namespace DesktopApplicationTemplate.UI.Services;

/// <summary>
/// Bridges UI view-model state with the core CSV service implementation.
/// </summary>
public class CsvServiceAdapter
{
    private readonly CsvViewerViewModel _viewModel;
    private readonly ProtocolCsvService _csvService;
    private readonly ICsvOutput _output;
    private readonly CsvServiceState _state = new();
    private readonly ILoggingService? _logger;

    public CsvServiceAdapter(CsvViewerViewModel viewModel, ProtocolCsvService csvService, ICsvOutput output, ILoggingService? logger = null)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _csvService = csvService ?? throw new ArgumentNullException(nameof(csvService));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _logger = logger;
    }

    public void EnsureColumnsForService(string serviceName)
    {
        if (_csvService.EnsureColumnsForService(Configuration, serviceName))
        {
            _viewModel.Save();
        }
    }

    public void RemoveColumnsForService(string serviceName)
    {
        var existingPath = GetCurrentFilePath();
        if (_csvService.RemoveColumnsForService(Configuration, _state, serviceName))
        {
            _viewModel.Save();
            if (!string.IsNullOrWhiteSpace(existingPath))
            {
                _output.DeleteFile(existingPath);
                if (_output.FileExists(existingPath))
                {
                    _logger?.Log($"CSV service attempted to delete file '{existingPath}' for '{serviceName}', but it still exists.", LogLevel.Warning);
                }
                else
                {
                    _logger?.Log($"CSV service deleted file '{existingPath}' for '{serviceName}'.", LogLevel.Information);
                }
            }
        }
    }

    public void RecordLog(string serviceName, string message)
    {
        var previousPath = GetCurrentFilePath();
        var fileExisted = !string.IsNullOrWhiteSpace(previousPath) && _output.FileExists(previousPath);

        _csvService.RecordLog(Configuration, _state, _output, serviceName, message);

        var currentPath = GetCurrentFilePath();
        if (string.IsNullOrWhiteSpace(currentPath))
        {
            return;
        }

        if (!fileExisted && _output.FileExists(currentPath))
        {
            _logger?.Log($"CSV service created file '{currentPath}' for '{serviceName}'.", LogLevel.Information);
        }

        if (_output.FileExists(currentPath))
        {
            _logger?.Log($"CSV service updated file '{currentPath}' with entry from '{serviceName}'.", LogLevel.Information);
        }
    }

    public void AppendRow(IEnumerable<string?> values)
    {
        _csvService.AppendRow(Configuration, _state, _output, values);
    }

    private CsvConfiguration Configuration => _viewModel.Configuration;

    private string? GetCurrentFilePath()
    {
        if (string.IsNullOrWhiteSpace(_state.CurrentFileName))
        {
            return null;
        }

        var directory = Configuration.OutputDirectory ?? string.Empty;
        return Path.Combine(directory, _state.CurrentFileName);
    }
}
