using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;
using MQTTnet.Protocol;

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
    private bool _useTls;
    private string? _clientCertificatePath;
    private string? _willTopic;
    private string? _willPayload;
    private MqttQualityOfServiceLevel _willQualityOfService = MqttQualityOfServiceLevel.AtMostOnce;
    private bool _willRetain;
    private ushort _keepAliveSeconds = 60;
    private bool _cleanSession = true;
    private int _reconnectDelaySeconds;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttCreateServiceViewModel"/> class.
    /// </summary>
    public MqttCreateServiceViewModel(ILoggingService? logger = null)
    {
        Logger = logger;
        CreateCommand = new RelayCommand(Create);
        CancelCommand = new RelayCommand(Cancel);
        QoSLevels = Enum.GetValues(typeof(MqttQualityOfServiceLevel)).Cast<MqttQualityOfServiceLevel>().ToArray();
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
    /// Command to finalize service creation.
    /// </summary>
    public ICommand CreateCommand { get; }

    /// <summary>
    /// Command to cancel configuration.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Available MQTT quality of service levels.
    /// </summary>
    public IReadOnlyList<MqttQualityOfServiceLevel> QoSLevels { get; }

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
    /// Whether TLS should be used for the connection.
    /// </summary>
    public bool UseTls
    {
        get => _useTls;
        set { _useTls = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Path to the client certificate used for TLS authentication.
    /// </summary>
    public string? ClientCertificatePath
    {
        get => _clientCertificatePath;
        set { _clientCertificatePath = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Will topic published on unexpected disconnect.
    /// </summary>
    public string? WillTopic
    {
        get => _willTopic;
        set { _willTopic = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Will message payload.
    /// </summary>
    public string? WillPayload
    {
        get => _willPayload;
        set { _willPayload = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Quality of service level for the will message.
    /// </summary>
    public MqttQualityOfServiceLevel WillQualityOfService
    {
        get => _willQualityOfService;
        set { _willQualityOfService = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Whether the will message should be retained.
    /// </summary>
    public bool WillRetain
    {
        get => _willRetain;
        set { _willRetain = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Keep alive interval in seconds.
    /// </summary>
    public ushort KeepAliveSeconds
    {
        get => _keepAliveSeconds;
        set { _keepAliveSeconds = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Whether a clean session should be used.
    /// </summary>
    public bool CleanSession
    {
        get => _cleanSession;
        set { _cleanSession = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Reconnect delay in seconds.
    /// </summary>
    public int ReconnectDelaySeconds
    {
        get => _reconnectDelaySeconds;
        set { _reconnectDelaySeconds = value; OnPropertyChanged(); }
    }

    private void Create()
    {
        Logger?.Log("MQTT create options start", LogLevel.Debug);
        var options = new MqttServiceOptions
        {
            Host = Host,
            Port = Port,
            ClientId = ClientId,
            Username = string.IsNullOrWhiteSpace(Username) ? null : Username,
            Password = string.IsNullOrWhiteSpace(Password) ? null : Password,
            UseTls = UseTls,
            WillTopic = string.IsNullOrWhiteSpace(WillTopic) ? null : WillTopic,
            WillPayload = string.IsNullOrWhiteSpace(WillPayload) ? null : WillPayload,
            WillQualityOfService = WillQualityOfService,
            WillRetain = WillRetain,
            KeepAliveSeconds = KeepAliveSeconds,
            CleanSession = CleanSession,
            ReconnectDelay = ReconnectDelaySeconds > 0 ? TimeSpan.FromSeconds(ReconnectDelaySeconds) : null
        };

        if (!string.IsNullOrWhiteSpace(ClientCertificatePath) && File.Exists(ClientCertificatePath))
        {
            options.ClientCertificate = File.ReadAllBytes(ClientCertificatePath);
        }

        Logger?.Log("MQTT create options finished", LogLevel.Debug);
        ServiceCreated?.Invoke(ServiceName, options);
    }

    private void Cancel()
    {
        Logger?.Log("MQTT create options cancelled", LogLevel.Debug);
        Cancelled?.Invoke();
    }
}
