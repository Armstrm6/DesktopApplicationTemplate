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
    private readonly ScpServiceOptions _options;
    private string _localPath;
    private string _remotePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScpAdvancedConfigViewModel"/> class.
    /// </summary>
    public ScpAdvancedConfigViewModel(ScpServiceOptions options, ILoggingService? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _localPath = options.LocalPath;
        _remotePath = options.RemotePath;
        Logger = logger;
        SaveCommand = new RelayCommand(Save);
        BackCommand = new RelayCommand(Back);
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
