using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for creating a new File Observer service.
/// </summary>
public class FileObserverCreateServiceViewModel : ServiceCreateViewModelBase<FileObserverServiceOptions>
{
    private string _serviceName = string.Empty;
    private string _filePath = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileObserverCreateServiceViewModel"/> class.
    /// </summary>
    public FileObserverCreateServiceViewModel(ILoggingService? logger = null)
        : base(logger)
    {
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
        set { _serviceName = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// File path to observe.
    /// </summary>
    public string FilePath
    {
        get => _filePath;
        set { _filePath = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Current configuration options.
    /// </summary>
    public FileObserverServiceOptions Options { get; } = new();

    /// <inheritdoc />
    protected override void OnSave()
    {
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
}
