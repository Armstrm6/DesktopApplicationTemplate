using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for creating a new Heartbeat service.
/// </summary>
public class HeartbeatCreateServiceViewModel : ViewModelBase, ILoggingViewModel
{
    private string _serviceName = string.Empty;
    private string _baseMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeartbeatCreateServiceViewModel"/> class.
    /// </summary>
    public HeartbeatCreateServiceViewModel(ILoggingService? logger = null)
    {
        Logger = logger;
        CreateCommand = new RelayCommand(Create);
        CancelCommand = new RelayCommand(Cancel);
        AdvancedConfigCommand = new RelayCommand(OpenAdvancedConfig);
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Raised when a new service configuration is saved.
    /// </summary>
    public event Action<string, HeartbeatServiceOptions>? ServiceCreated;

    /// <summary>
    /// Raised when creation is cancelled.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<HeartbeatServiceOptions>? AdvancedConfigRequested;

    /// <summary>
    /// Command to create the service.
    /// </summary>
    public ICommand CreateCommand { get; }

    /// <summary>
    /// Command to cancel creation.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Command to open advanced configuration.
    /// </summary>
    public ICommand AdvancedConfigCommand { get; }

    /// <summary>
    /// Name of the service.
    /// </summary>
    public string ServiceName
    {
        get => _serviceName;
        set { _serviceName = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Base message for the heartbeat.
    /// </summary>
    public string BaseMessage
    {
        get => _baseMessage;
        set { _baseMessage = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Current configuration options.
    /// </summary>
    public HeartbeatServiceOptions Options { get; } = new();

    private void Create()
    {
        Logger?.Log("Heartbeat create options start", LogLevel.Debug);
        Options.BaseMessage = BaseMessage;
        Logger?.Log("Heartbeat create options finished", LogLevel.Debug);
        ServiceCreated?.Invoke(ServiceName, Options);
    }

    private void Cancel()
    {
        Logger?.Log("Heartbeat create cancelled", LogLevel.Debug);
        Cancelled?.Invoke();
    }

    private void OpenAdvancedConfig()
    {
        Logger?.Log("Heartbeat advanced config requested", LogLevel.Debug);
        Options.BaseMessage = BaseMessage;
        AdvancedConfigRequested?.Invoke(Options);
    }
}
