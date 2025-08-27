using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for configuring a new TCP service before creation.
/// </summary>
public class TcpCreateServiceViewModel : ViewModelBase
{
    private string _serviceName = string.Empty;
    private string _host = string.Empty;
    private int _port = 0;

    /// <summary>
    /// Current advanced options.
    /// </summary>
    public TcpServiceOptions Options { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpCreateServiceViewModel"/> class.
    /// </summary>
    public TcpCreateServiceViewModel(ILoggingService? logger = null)
    {
        Logger = logger;
        CreateCommand = new RelayCommand(Create);
        CancelCommand = new RelayCommand(Cancel);
        AdvancedConfigCommand = new RelayCommand(OpenAdvancedConfig);
    }

    /// <summary>
    /// Raised when the user finishes configuring the service.
    /// </summary>
    public event Action<string, TcpServiceOptions>? ServiceCreated;

    /// <summary>
    /// Raised when the user cancels configuration.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Command to finalize service creation.
    /// </summary>
    public ICommand CreateCommand { get; }

    /// <summary>
    /// Command to cancel configuration.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Command to open advanced configuration.
    /// </summary>
    public ICommand AdvancedConfigCommand { get; }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Name of the service to create.
    /// </summary>
    public string ServiceName
    {
        get => _serviceName;
        set { _serviceName = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Host name or address for the service.
    /// </summary>
    public string Host
    {
        get => _host;
        set { _host = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Port number for the service.
    /// </summary>
    public int Port
    {
        get => _port;
        set { _port = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<TcpServiceOptions>? AdvancedConfigRequested;

    private void Create()
    {
        Logger?.Log("TCP create options start", LogLevel.Debug);
        Options.Host = Host;
        Options.Port = Port;
        Logger?.Log("TCP create options finished", LogLevel.Debug);
        ServiceCreated?.Invoke(ServiceName, Options);
    }

    private void Cancel()
    {
        Logger?.Log("TCP create options cancelled", LogLevel.Debug);
        Cancelled?.Invoke();
    }

    private void OpenAdvancedConfig()
    {
        Logger?.Log("Opening TCP advanced config", LogLevel.Debug);
        Options.Host = Host;
        Options.Port = Port;
        AdvancedConfigRequested?.Invoke(Options);
    }
}
