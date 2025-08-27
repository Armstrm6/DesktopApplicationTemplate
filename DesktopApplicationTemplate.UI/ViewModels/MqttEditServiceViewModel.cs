using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing an existing MQTT service configuration.
/// </summary>
public class MqttEditServiceViewModel : ServiceEditViewModelBase<MqttServiceOptions>
{
    private readonly MqttServiceOptions _options;
    private string _serviceName;
    private string _host;
    private int _port;
    private string _clientId;
    private string? _username;
    private string? _password;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttEditServiceViewModel"/> class.
    /// </summary>
    public MqttEditServiceViewModel(string serviceName, MqttServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        _host = options.Host;
        _port = options.Port;
        _clientId = options.ClientId;
        _username = options.Username;
        _password = options.Password;
    }

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<string, MqttServiceOptions>? ServiceUpdated;

    /// <summary>
    /// Raised when editing is cancelled.
    /// </summary>
    public event Action? Cancelled;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<MqttServiceOptions>? AdvancedConfigRequested;

    /// <summary>
    /// Name of the service.
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

    /// <inheritdoc />
    protected override void OnSave()
    {
        _options.Host = Host;
        _options.Port = Port;
        _options.ClientId = ClientId;
        _options.Username = string.IsNullOrWhiteSpace(Username) ? null : Username;
        _options.Password = string.IsNullOrWhiteSpace(Password) ? null : Password;
        ServiceUpdated?.Invoke(ServiceName, _options);
    }

    /// <inheritdoc />
    protected override void OnCancel() => Cancelled?.Invoke();

    /// <inheritdoc />
    protected override void OnAdvancedConfig() => AdvancedConfigRequested?.Invoke(_options);
}

