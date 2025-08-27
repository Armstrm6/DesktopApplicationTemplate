using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing an existing CSV creator service configuration.
/// </summary>
public class CsvEditServiceViewModel : ServiceEditViewModelBase<CsvServiceOptions>
{
    private readonly CsvServiceOptions _options;
    private string _serviceName;
    private string _outputPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvEditServiceViewModel"/> class.
    /// </summary>
    public CsvEditServiceViewModel(string serviceName, CsvServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        _outputPath = options.OutputPath;
        Options = options;
    }

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<string, CsvServiceOptions>? ServiceUpdated;

    /// <summary>
    /// Raised when editing is cancelled.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<CsvServiceOptions>? AdvancedConfigRequested;

    /// <summary>
    /// Display name for the service.
    /// </summary>
    public string ServiceName
    {
        get => _serviceName;
        set
        {
            _serviceName = value;
            if (string.IsNullOrWhiteSpace(value))
                AddError(nameof(ServiceName), "Service name is required");
            else
                ClearErrors(nameof(ServiceName));
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Output directory for CSV files.
    /// </summary>
    public string OutputPath
    {
        get => _outputPath;
        set
        {
            _outputPath = value;
            if (string.IsNullOrWhiteSpace(value))
                AddError(nameof(OutputPath), "Output path is required");
            else
                ClearErrors(nameof(OutputPath));
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Current configuration options.
    /// </summary>
    public CsvServiceOptions Options { get; private set; }

    /// <inheritdoc />
    protected override void OnSave()
    {
        if (HasErrors)
            return;
        _options.OutputPath = OutputPath;
        _options.Delimiter = Options.Delimiter;
        _options.IncludeHeaders = Options.IncludeHeaders;
        ServiceUpdated?.Invoke(ServiceName, _options);
    }

    /// <inheritdoc />
    protected override void OnCancel() => Cancelled?.Invoke();

    /// <inheritdoc />
    protected override void OnAdvancedConfig() => AdvancedConfigRequested?.Invoke(_options);
}

