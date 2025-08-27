using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing advanced SCP configuration.
/// </summary>
public class ScpAdvancedConfigViewModel : ViewModelBase, ILoggingViewModel
{
    private ScpServiceOptions? _options;
    private string _localPath = string.Empty;
    private string _remotePath = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScpAdvancedConfigViewModel"/> class.
    /// </summary>
    public ScpAdvancedConfigViewModel(ILoggingService? logger = null)
    {
        Logger = logger;
        SaveCommand = new RelayCommand(Save);
        BackCommand = new RelayCommand(Back);
    }

    /// <summary>
    /// Loads existing options into the view model.
    /// </summary>
    /// <param name="options">Options to edit.</param>
    public void Load(ScpServiceOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _localPath = options.LocalPath;
        _remotePath = options.RemotePath;
        OnPropertyChanged(nameof(LocalPath));
        OnPropertyChanged(nameof(RemotePath));
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Command to save the configuration.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Command to navigate back without saving.
    /// </summary>
    public ICommand BackCommand { get; }

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<ScpServiceOptions>? Saved;

    /// <summary>
    /// Raised when navigation back is requested.
    /// </summary>
    public event Action? BackRequested;

    /// <summary>
    /// Local file path to upload.
    /// </summary>
    public string LocalPath
    {
        get => _localPath;
        set { _localPath = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Remote destination path.
    /// </summary>
    public string RemotePath
    {
        get => _remotePath;
        set { _remotePath = value; OnPropertyChanged(); }
    }

    private void Save()
    {
        Logger?.Log("SCP advanced options start", LogLevel.Debug);
        if (_options is null) throw new InvalidOperationException("Options not loaded");
        _options.LocalPath = LocalPath;
        _options.RemotePath = RemotePath;
        Logger?.Log("SCP advanced options finished", LogLevel.Debug);
        Saved?.Invoke(_options);
    }

    private void Back()
    {
        Logger?.Log("SCP advanced options back", LogLevel.Debug);
        BackRequested?.Invoke();
    }
}
