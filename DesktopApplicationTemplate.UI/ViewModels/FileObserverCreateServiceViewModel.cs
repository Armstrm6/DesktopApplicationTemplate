using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for creating a new File Observer service.
/// </summary>
public class FileObserverCreateServiceViewModel : ServiceCreateViewModelBase<FileObserverServiceOptions>
{
    private readonly IServiceRule _rule;
    private readonly IFileDialogService _fileDialog;
    private string _serviceName = string.Empty;
    private string _filePath = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileObserverCreateServiceViewModel"/> class.
    /// </summary>
    public FileObserverCreateServiceViewModel(IServiceRule rule, IFileDialogService fileDialog, ILoggingService? logger = null)
        : base(logger)
    {
        _rule = rule ?? throw new ArgumentNullException(nameof(rule));
        _fileDialog = fileDialog ?? throw new ArgumentNullException(nameof(fileDialog));
        BrowseCommand = new RelayCommand(BrowseFolder);
    }

    /// <summary>
    /// Raised when the service is created.
    /// </summary>
    public event Action<string, FileObserverServiceOptions>? ServiceCreated;

    /// <summary>
    /// Raised when creation is cancelled.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<FileObserverServiceOptions>? AdvancedConfigRequested;

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
    /// File path to observe.
    /// </summary>
    public string FilePath
    {
        get => _filePath;
        set
        {
            _filePath = value;
            var error = _rule.ValidateRequired(value, "File path");
            if (error is not null)
                AddError(nameof(FilePath), error);
            else
                ClearErrors(nameof(FilePath));
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Command to browse for a folder.
    /// </summary>
    public ICommand BrowseCommand { get; }

    /// <summary>
    /// Current configuration options.
    /// </summary>
    public FileObserverServiceOptions Options { get; } = new();

    /// <inheritdoc />
    protected override void OnSave()
    {
        if (HasErrors)
        {
            Logger?.Log("FileObserver create validation failed", LogLevel.Warning);
            return;
        }
        Logger?.Log("FileObserver create options start", LogLevel.Debug);
        Options.FilePath = FilePath;
        Logger?.Log("FileObserver create options finished", LogLevel.Debug);
        ServiceCreated?.Invoke(ServiceName, Options);
    }

    /// <inheritdoc />
    protected override void OnCancel()
    {
        Logger?.Log("FileObserver create cancelled", LogLevel.Debug);
        Cancelled?.Invoke();
    }

    /// <inheritdoc />
    protected override void OnAdvancedConfig()
    {
        Logger?.Log("Opening FileObserver advanced config", LogLevel.Debug);
        Options.FilePath = FilePath;
        AdvancedConfigRequested?.Invoke(Options);
    }

    private void BrowseFolder()
    {
        var path = _fileDialog.SelectFolder();
        if (!string.IsNullOrWhiteSpace(path))
        {
            FilePath = path;
        }
    }
}
