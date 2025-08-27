using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for configuring a new MQTT service before creation.
/// </summary>
public class MqttCreateServiceViewModel : ViewModelBase
{
    private string _serviceName = string.Empty;
    private string _host = string.Empty;
    private int _port = 1883;
    private string _clientId = string.Empty;
    private string? _username;
    private string? _password;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttCreateServiceViewModel"/> class.
    /// </summary>
    public MqttCreateServiceViewModel(ILoggingService? logger = null)
    {
        Logger = logger;
        CreateCommand = new RelayCommand(Create);
        CancelCommand = new RelayCommand(Cancel);
        AdvancedConfigCommand = new RelayCommand(OpenAdvancedConfig);
    }

    /// <summary>
    /// Raised when the user finishes configuring the service.
    /// </summary>
    public event Action<string, MqttServiceOptions>? ServiceCreated;

    /// <summary>
    /// Raised when the user cancels configuration.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<MqttServiceOptions>? AdvancedConfigRequested;

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
    /// MQTT broker host.
    /// </summary>
    public string Host
    {
        get => _host;
        set { _host = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// MQTT broker port.
    /// </summary>
    public int Port
    {
        get => _port;
        set { _port = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Client identifier used to connect to the broker.
    /// </summary>
    public string ClientId
    {
        get => _clientId;
        set { _clientId = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Username for authentication.
    /// </summary>
    public string? Username
    {
        get => _username;
        set { _username = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Password for authentication.
    /// </summary>
    public string? Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Current configuration options including advanced settings.
    /// </summary>
    public MqttServiceOptions Options { get; } = new();

    private void Create()
    {
        Logger?.Log("MQTT create options start", LogLevel.Debug);
        Options.Host = Host;
        Options.Port = Port;
        Options.ClientId = ClientId;
        Options.Username = string.IsNullOrWhiteSpace(Username) ? null : Username;
        Options.Password = string.IsNullOrWhiteSpace(Password) ? null : Password;
        Options.WillTopic = string.IsNullOrWhiteSpace(Options.WillTopic) ? null : Options.WillTopic;
        Options.WillPayload = string.IsNullOrWhiteSpace(Options.WillPayload) ? null : Options.WillPayload;
        Logger?.Log("MQTT create options finished", LogLevel.Debug);
        ServiceCreated?.Invoke(ServiceName, Options);
    }

    private void Cancel()
    {
        Logger?.Log("MQTT create options cancelled", LogLevel.Debug);
        Cancelled?.Invoke();
    }

    private void OpenAdvancedConfig()
    {
        Logger?.Log("Opening MQTT advanced config", LogLevel.Debug);
        Options.Host = Host;
        Options.Port = Port;
        Options.ClientId = ClientId;
        Options.Username = string.IsNullOrWhiteSpace(Username) ? null : Username;
        Options.Password = string.IsNullOrWhiteSpace(Password) ? null : Password;
        Options.WillTopic = string.IsNullOrWhiteSpace(Options.WillTopic) ? null : Options.WillTopic;
        Options.WillPayload = string.IsNullOrWhiteSpace(Options.WillPayload) ? null : Options.WillPayload;
        AdvancedConfigRequested?.Invoke(Options);
    }
}
