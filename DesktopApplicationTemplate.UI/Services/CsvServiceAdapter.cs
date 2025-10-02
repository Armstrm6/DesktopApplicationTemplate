using System;
using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Services.Protocols.Csv;
using DesktopApplicationTemplate.UI.ViewModels.Csv;

namespace DesktopApplicationTemplate.UI.Services;

/// <summary>
/// Bridges UI view-model state with the core CSV service implementation.
/// </summary>
public class CsvServiceAdapter
{
    private readonly CsvViewerViewModel _viewModel;
    private readonly ICsvService _csvService;
    private readonly ICsvOutput _output;
    private readonly CsvServiceState _state = new();

    public CsvServiceAdapter(CsvViewerViewModel viewModel, ICsvService csvService, ICsvOutput output)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _csvService = csvService ?? throw new ArgumentNullException(nameof(csvService));
        _output = output ?? throw new ArgumentNullException(nameof(output));
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
        if (_csvService.RemoveColumnsForService(Configuration, _state, serviceName))
        {
            _viewModel.Save();
        }
    }

    public void RecordLog(string serviceName, string message)
    {
        _csvService.RecordLog(Configuration, _state, _output, serviceName, message);
    }

    public void AppendRow(IEnumerable<string?> values)
    {
        _csvService.AppendRow(Configuration, _state, _output, values);
    }

    private CsvConfiguration Configuration => _viewModel.Configuration;
}
