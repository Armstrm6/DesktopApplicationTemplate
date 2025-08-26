using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing a file observer service.
/// </summary>
public class FileObserverEditServiceViewModel : ViewModelBase, ILoggingViewModel
{
    private string _serviceName;
    private string _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileObserverEditServiceViewModel"/> class.
    /// </summary>
    public FileObserverEditServiceViewModel(string serviceName, FileObserverOptions options, ILoggingService? logger = null)
    {
        _serviceName = serviceName;
        _path = options.Path;
        Options = options;
        Logger = logger;
        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
        OpenAdvancedConfigCommand = new RelayCommand(OpenAdvancedConfig);
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Raised when the service is updated.
    /// </summary>
    public event Action<string, FileObserverOptions>? ServiceUpdated;

    /// <summary>
    /// Raised when editing is cancelled.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<FileObserverOptions>? AdvancedConfigRequested;

    /// <summary>
    /// Name of the service.
    /// </summary>
    public string ServiceName
    {
        get => _serviceName;
        set { _serviceName = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Path being observed.
    /// </summary>
    public string Path
    {
        get => _path;
        set { _path = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Options for the service.
    /// </summary>
    public FileObserverOptions Options { get; }

    /// <summary>
    /// Command to save changes.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Command to cancel editing.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Command to open advanced configuration.
    /// </summary>
    public ICommand OpenAdvancedConfigCommand { get; }

    private void Save()
    {
        Logger?.Log("File observer edit options start", LogLevel.Debug);
        Options.Path = Path;
        Logger?.Log("File observer edit options finished", LogLevel.Debug);
        ServiceUpdated?.Invoke(ServiceName, Options);
    }

    private void Cancel()
    {
        Logger?.Log("File observer edit cancelled", LogLevel.Debug);
        Cancelled?.Invoke();
    }

    private void OpenAdvancedConfig()
    {
        Logger?.Log("Opening file observer advanced config", LogLevel.Debug);
        Options.Path = Path;
        AdvancedConfigRequested?.Invoke(Options);
    }
}

