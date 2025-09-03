using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for creating or editing a CSV creator service.
/// </summary>
public class CsvServiceEditorViewModel : ServiceEditorViewModelBase<CsvServiceOptions>
{
    private readonly IServiceScreen<CsvServiceOptions> _screen;
    private string _outputPath = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvServiceEditorViewModel"/> class.
    /// Defaults to create mode with Save button labeled "Create".
    /// </summary>
    public CsvServiceEditorViewModel(IServiceRule rule, IServiceScreen<CsvServiceOptions> screen, ILoggingService? logger = null)
        : base(rule, logger)
    {
        _screen = screen ?? throw new ArgumentNullException(nameof(screen));
        SaveButtonText = "Create";
        Options = new();
        _screen.ServiceSaved += (_, o) => RaiseServiceSaved(o);
        _screen.EditCancelled += () => RaiseEditCancelled();
        _screen.AdvancedConfigRequested += o => RaiseAdvancedConfigRequested(o);
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
            var error = Rule.ValidateRequired(value, "Output path");
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
        ServiceName = serviceName;
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
