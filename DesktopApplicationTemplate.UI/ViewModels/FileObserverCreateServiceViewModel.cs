using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for creating a file observer service.
/// </summary>
public class FileObserverCreateServiceViewModel : ViewModelBase, ILoggingViewModel
{
    private string _serviceName = string.Empty;
    private string _path = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileObserverCreateServiceViewModel"/> class.
    /// </summary>
    public FileObserverCreateServiceViewModel(ILoggingService? logger = null)
    {
        Logger = logger;
        CreateCommand = new RelayCommand(Create);
        CancelCommand = new RelayCommand(Cancel);
        OpenAdvancedConfigCommand = new RelayCommand(OpenAdvancedConfig);
    }

    /// <summary>
    /// Raised when the service is created.
    /// </summary>
    public event Action<string, FileObserverOptions>? ServiceCreated;

    /// <summary>
    /// Raised when creation is cancelled.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<FileObserverOptions>? AdvancedConfigRequested;

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Name of the service.
    /// </summary>
    public string ServiceName
    {
        get => _serviceName;
        set { _serviceName = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Path to observe.
    /// </summary>
    public string Path
    {
        get => _path;
        set { _path = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Current configuration options.
    /// </summary>
    public FileObserverOptions Options { get; } = new();

    /// <summary>
    /// Command for creating the service.
    /// </summary>
    public ICommand CreateCommand { get; }

    /// <summary>
    /// Command for cancelling creation.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Command to open advanced configuration.
    /// </summary>
    public ICommand OpenAdvancedConfigCommand { get; }

    private void Create()
    {
        Logger?.Log("File observer create options start", LogLevel.Debug);
        Options.Path = Path;
        Logger?.Log("File observer create options finished", LogLevel.Debug);
        ServiceCreated?.Invoke(ServiceName, Options);
    }

    private void Cancel()
    {
        Logger?.Log("File observer create cancelled", LogLevel.Debug);
        Cancelled?.Invoke();
    }

    private void OpenAdvancedConfig()
    {
        Logger?.Log("Opening file observer advanced config", LogLevel.Debug);
        Options.Path = Path;
        AdvancedConfigRequested?.Invoke(Options);
    }
}

