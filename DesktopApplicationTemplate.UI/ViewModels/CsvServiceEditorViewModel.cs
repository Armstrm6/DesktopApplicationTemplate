using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for creating or editing a CSV creator service.
/// </summary>
public class CsvServiceEditorViewModel : ServiceEditorViewModelBase<CsvServiceOptions>
{
    private readonly IServiceRule _rule;
    private readonly IServiceScreen<CsvServiceOptions> _screen;
    private string _serviceName = string.Empty;
    private string _outputPath = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvServiceEditorViewModel"/> class.
    /// Defaults to create mode with Save button labeled "Create".
    /// </summary>
    public CsvServiceEditorViewModel(IServiceRule rule, IServiceScreen<CsvServiceOptions> screen, ILoggingService? logger = null)
        : base(logger)
    {
        _rule = rule ?? throw new ArgumentNullException(nameof(rule));
        _screen = screen ?? throw new ArgumentNullException(nameof(screen));
        SaveButtonText = "Create";
        Options = new();
        _screen.Saved += (n, o) => ServiceSaved?.Invoke(n, o);
        _screen.Cancelled += () => Cancelled?.Invoke();
        _screen.AdvancedConfigRequested += o => AdvancedConfigRequested?.Invoke(o);
    }

    /// <summary>
    /// Raised when the service is saved.
    /// </summary>
    public event Action<string, CsvServiceOptions>? ServiceSaved;

    /// <summary>
    /// Raised when editing is cancelled.
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
    public CsvServiceOptions Options { get; private set; }

    /// <summary>
    /// Loads existing options for edit workflows.
    /// </summary>
    public void Load(string serviceName, CsvServiceOptions options)
    {
        SaveButtonText = "Save";
        _serviceName = serviceName;
        _outputPath = options.OutputPath;
        Options = options;
        OnPropertyChanged(nameof(ServiceName));
        OnPropertyChanged(nameof(OutputPath));
    }

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
