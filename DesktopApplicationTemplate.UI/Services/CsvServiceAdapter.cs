using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    private readonly CsvServiceViewModel _viewModel;
    private readonly ProtocolCsvService _csvService;
    private readonly ICsvOutput _output;
    private readonly CsvServiceState _state = new();
    private readonly ILoggingService? _logger;
    private readonly IMessageRoutingService _routingService;
    private readonly object _syncRoot = new();

    public CsvServiceAdapter(
        CsvServiceViewModel viewModel,
        ProtocolCsvService csvService,
        ICsvOutput output,
        IMessageRoutingService routingService,
        ILoggingService? logger = null)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _csvService = csvService ?? throw new ArgumentNullException(nameof(csvService));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
        _logger = logger;
        _routingService.AttributeChanged += OnRoutingAttributeChanged;
    }

    public void EnsureColumnsForService(string serviceName)
    {
        _csvService.EnsureColumnsForService(Configuration, serviceName);
    }

    public async Task RemoveColumnsForServiceAsync(string serviceName)
    {
        var existingPath = GetCurrentFilePath();
        if (_csvService.RemoveColumnsForService(Configuration, _state, serviceName))
        {
            await _viewModel.SaveAsync().ConfigureAwait(false);
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

        WriteSnapshot(serviceName, message);

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
        lock (_syncRoot)
        {
            _csvService.AppendRow(Configuration, _state, _output, values);
        }
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

    private void OnRoutingAttributeChanged(object? sender, ServiceAttributeChangedEventArgs e)
    {
        if (e is null)
        {
            return;
        }

        if (!IsExpressionReferencing(e.ServiceName, e.AttributeName))
        {
            return;
        }

        WriteSnapshot(e.ServiceName, e.Value ?? string.Empty);
    }

    private bool IsExpressionReferencing(string serviceName, string attributeName)
    {
        if (string.IsNullOrWhiteSpace(serviceName) || string.IsNullOrWhiteSpace(attributeName))
        {
            return false;
        }

        return Configuration.Columns.Any(column =>
            CsvExpressionEvaluator.ReferencesAttribute(column.Expression, serviceName, attributeName));
    }

    private void WriteSnapshot(string serviceName, string message)
    {
        lock (_syncRoot)
        {
            _csvService.RecordLog(Configuration, _state, _output, serviceName, message);
        }
    }
}
