using System;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Csv;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels.Csv.Edit;

/// <summary>
/// View model for creating or editing a CSV creator service.
/// </summary>
public class CsvServiceEditorViewModel : ServiceEditorViewModelBase<CsvServiceOptions>
{
    private readonly IServiceScreen<CsvServiceOptions> _screen;
    private readonly IFileDialogService _fileDialog;
    private string _outputPath = string.Empty;
    private string _delimiter = ",";
    private bool _includeHeaders = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvServiceEditorViewModel"/> class.
    /// Defaults to create mode with Save button labeled "Create".
    /// </summary>
    public CsvServiceEditorViewModel(
        IServiceRule rule,
        IServiceScreen<CsvServiceOptions> screen,
        IFileDialogService fileDialog,
        ILoggingService? logger = null)
        : base(rule, logger)
    {
        _screen = screen ?? throw new ArgumentNullException(nameof(screen));
        _fileDialog = fileDialog ?? throw new ArgumentNullException(nameof(fileDialog));
        SaveButtonText = "Create";
        Options = new();
        _delimiter = Options.Delimiter;
        _includeHeaders = Options.IncludeHeaders;
        _screen.ServiceSaved += (_, o) => RaiseServiceSavedAsync(o);
        _screen.EditCancelled += () => RaiseEditCancelled();
        BrowseCommand = new RelayCommand(BrowseForOutputPath);
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
    /// Delimiter used between values.
    /// </summary>
    public string Delimiter
    {
        get => _delimiter;
        set
        {
            _delimiter = value;
            var error = Rule.ValidateRequired(value, "Delimiter");
            if (error is not null)
                AddError(nameof(Delimiter), error);
            else
                ClearErrors(nameof(Delimiter));
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Indicates whether headers should be included in generated files.
    /// </summary>
    public bool IncludeHeaders
    {
        get => _includeHeaders;
        set
        {
            _includeHeaders = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Command that allows the user to browse for an output directory.
    /// </summary>
    public ICommand BrowseCommand { get; }

    private void BrowseForOutputPath()
    {
        var path = _fileDialog.SelectFolder();
        if (!string.IsNullOrWhiteSpace(path))
        {
            OutputPath = path;
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
        Options = options;
        OutputPath = options.OutputPath;
        Delimiter = options.Delimiter;
        IncludeHeaders = options.IncludeHeaders;
        OnPropertyChanged(nameof(ServiceName));
    }

    /// <inheritdoc />
    protected override async Task OnSaveAsync()
    {
        if (HasErrors)
            return;
        Options.OutputPath = OutputPath;
        Options.Delimiter = Delimiter;
        Options.IncludeHeaders = IncludeHeaders;
        await _screen.SaveAsync(ServiceName, Options);
    }

    /// <inheritdoc />
    protected override void OnCancel() => _screen.Cancel();

    /// <inheritdoc />
    protected override void OnAdvancedConfig()
    {
        // CSV services no longer expose a separate advanced configuration view.
    }
}
