using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for creating a new CSV creator service.
/// </summary>
public class CsvCreateServiceViewModel : ServiceCreateViewModelBase<CsvServiceOptions>
{
    private readonly IServiceRule _rule;
    private readonly IServiceScreen<CsvServiceOptions> _screen;
    private string _serviceName = string.Empty;
    private string _outputPath = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvCreateServiceViewModel"/> class.
    /// </summary>
    public CsvCreateServiceViewModel(IServiceRule rule, IServiceScreen<CsvServiceOptions> screen, ILoggingService? logger = null)
        : base(logger)
    {
        _rule = rule ?? throw new ArgumentNullException(nameof(rule));
        _screen = screen ?? throw new ArgumentNullException(nameof(screen));

        _screen.Saved += (n, o) => ServiceCreated?.Invoke(n, o);
        _screen.Cancelled += () => Cancelled?.Invoke();
        _screen.AdvancedConfigRequested += o => AdvancedConfigRequested?.Invoke(o);
    }

    /// <summary>
    /// Raised when the service is created.
    /// </summary>
    public event Action<string, CsvServiceOptions>? ServiceCreated;

    /// <summary>
    /// Raised when creation is cancelled.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<CsvServiceOptions>? AdvancedConfigRequested;

    /// <summary>
    /// Name of the service.
    /// </summary>
    public string ServiceName
    {
        get => _serviceName;
        set
        {
            _serviceName = value;
            var error = _rule.ValidateRequired(value, "Service name");
            if (error is not null)
                AddError(nameof(ServiceName), error);
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
            var error = _rule.ValidateRequired(value, "Output path");
            if (error is not null)
                AddError(nameof(OutputPath), error);
            else
                ClearErrors(nameof(OutputPath));
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Current configuration options.
    /// </summary>
    public CsvServiceOptions Options { get; } = new();

    /// <inheritdoc />
    protected override void OnSave()
    {
        if (HasErrors)
            return;
        Options.OutputPath = OutputPath;
        _screen.Save(ServiceName, Options);
    }

    /// <inheritdoc />
    protected override void OnCancel() => _screen.Cancel();

    /// <inheritdoc />
    protected override void OnAdvancedConfig()
    {
        Options.OutputPath = OutputPath;
        _screen.OpenAdvanced(Options);
    }
}
