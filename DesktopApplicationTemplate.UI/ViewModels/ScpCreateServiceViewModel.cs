using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for creating a new SCP service.
/// </summary>
public class ScpCreateServiceViewModel : ViewModelBase, ILoggingViewModel
{
    private string _serviceName = string.Empty;
    private string _host = string.Empty;
    private string _port = "22";
    private string _username = string.Empty;
    private string _password = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScpCreateServiceViewModel"/> class.
    /// </summary>
    public ScpCreateServiceViewModel(ILoggingService? logger = null)
    {
        Logger = logger;
        CreateCommand = new RelayCommand(Create);
        CancelCommand = new RelayCommand(Cancel);
        AdvancedConfigCommand = new RelayCommand(OpenAdvancedConfig);
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Raised when the service is created.
    /// </summary>
    public event Action<string, ScpServiceOptions>? ServiceCreated;

    /// <summary>
    /// Raised when creation is cancelled.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<ScpServiceOptions>? AdvancedConfigRequested;

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
    /// SCP host name.
    /// </summary>
    public string Host
    {
        get => _host;
        set { _host = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// SCP port number.
    /// </summary>
    public string Port
    {
        get => _port;
        set { _port = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Username for authentication.
    /// </summary>
    public string Username
    {
        get => _username;
        set { _username = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Password for authentication.
    /// </summary>
    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Current configuration options.
    /// </summary>
    public ScpServiceOptions Options { get; } = new();

    private void Create()
    {
        Logger?.Log("SCP create options start", LogLevel.Debug);
        Options.Host = Host;
        if (int.TryParse(Port, out var port))
            Options.Port = port;
        Options.Username = Username;
        Options.Password = Password;
        Logger?.Log("SCP create options finished", LogLevel.Debug);
        ServiceCreated?.Invoke(ServiceName, Options);
    }

    private void Cancel()
    {
        Logger?.Log("SCP create cancelled", LogLevel.Debug);
        Cancelled?.Invoke();
    }

    private void OpenAdvancedConfig()
    {
        Logger?.Log("Opening SCP advanced config", LogLevel.Debug);
        Options.Host = Host;
        if (int.TryParse(Port, out var port))
            Options.Port = port;
        Options.Username = Username;
        Options.Password = Password;
        AdvancedConfigRequested?.Invoke(Options);
    }
}
