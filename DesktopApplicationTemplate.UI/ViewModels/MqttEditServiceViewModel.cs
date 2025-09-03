using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing an existing MQTT service configuration.
/// </summary>
public class MqttEditServiceViewModel : ServiceEditViewModelBase<MqttServiceOptions>
{
    private readonly IServiceRule _rule;
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
    public MqttEditServiceViewModel(IServiceRule rule, string serviceName, MqttServiceOptions options, ILoggingService? logger = null)
        : base(logger)
    {
        _rule = rule ?? throw new ArgumentNullException(nameof(rule));
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
        set
        {
            _serviceName = value;
            var error = _rule.ValidateRequired(value, "Service name");
            if (error is not null)
                AddError(nameof(ServiceName), error);
            else
                ClearErrors(nameof(ServiceName));
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// MQTT broker host.
    /// </summary>
    public string Host
    {
        get => _host;
        set
        {
            _host = value;
            var error = _rule.ValidateRequired(value, "Host");
            if (error is not null)
                AddError(nameof(Host), error);
            else
                ClearErrors(nameof(Host));
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// MQTT broker port.
    /// </summary>
    public int Port
    {
        get => _port;
        set
        {
            _port = value;
            var error = _rule.ValidatePort(value);
            if (error is not null)
                AddError(nameof(Port), error);
            else
                ClearErrors(nameof(Port));
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Client identifier used to connect to the broker.
    /// </summary>
    public string ClientId
    {
        get => _clientId;
        set
        {
            _clientId = value;
            var error = _rule.ValidateRequired(value, "Client Id");
            if (error is not null)
                AddError(nameof(ClientId), error);
            else
                ClearErrors(nameof(ClientId));
            OnPropertyChanged();
        }
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
        if (HasErrors)
        {
            Logger?.Log("MQTT edit validation failed", LogLevel.Warning);
            return;
        }
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

