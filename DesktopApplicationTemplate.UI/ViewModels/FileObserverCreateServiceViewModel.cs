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
    private readonly IFileDialogService _fileDialog;
    private string _filePath = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileObserverCreateServiceViewModel"/> class.
    /// </summary>
    public FileObserverCreateServiceViewModel(IServiceRule rule, IFileDialogService fileDialog, ILoggingService? logger = null)
        : base(rule, logger)
    {
        _fileDialog = fileDialog ?? throw new ArgumentNullException(nameof(fileDialog));
        BrowseCommand = new RelayCommand(BrowseFolder);
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
            var error = Rule.ValidateRequired(value, "File path");
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
        RaiseServiceSaved(Options);
    }

    /// <inheritdoc />
    protected override void OnCancel()
    {
        Logger?.Log("FileObserver create cancelled", LogLevel.Debug);
        RaiseEditCancelled();
    }

    /// <inheritdoc />
    protected override void OnAdvancedConfig()
    {
        Logger?.Log("Opening FileObserver advanced config", LogLevel.Debug);
        Options.FilePath = FilePath;
        RaiseAdvancedConfigRequested(Options);
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
