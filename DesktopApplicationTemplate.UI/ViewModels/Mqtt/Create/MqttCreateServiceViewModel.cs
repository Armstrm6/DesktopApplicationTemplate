using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;

namespace DesktopApplicationTemplate.UI.ViewModels.Mqtt.Create;

/// <summary>
/// View model for configuring a new MQTT service before creation.
/// </summary>
public class MqttCreateServiceViewModel : ServiceCreateViewModelBase<MqttServiceOptions>
{
    private string _host = string.Empty;
    private int _port = 1883;
    private string _clientId = string.Empty;
    private string? _username;
    private string? _password;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttCreateServiceViewModel"/> class.
    /// </summary>
    public MqttCreateServiceViewModel(IServiceRule rule, ILoggingService? logger = null)
        : base(rule, logger: logger)
    {
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
            var error = Rule.ValidateRequired(value, "Host");
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
            var error = Rule.ValidatePort(value);
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
            var error = Rule.ValidateRequired(value, "Client Id");
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
    protected override void ApplyOptions(MqttServiceOptions options)
    {
        options.Host = Host;
        options.Port = Port;
        options.ClientId = ClientId;
        options.Username = string.IsNullOrWhiteSpace(Username) ? null : Username;
        options.Password = string.IsNullOrWhiteSpace(Password) ? null : Password;
        options.WillTopic = string.IsNullOrWhiteSpace(options.WillTopic) ? null : options.WillTopic;
        options.WillPayload = string.IsNullOrWhiteSpace(options.WillPayload) ? null : options.WillPayload;
    }
}
