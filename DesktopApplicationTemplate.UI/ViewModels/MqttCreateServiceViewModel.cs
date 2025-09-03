using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

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
        : base(rule, logger)
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

    /// <summary>
    /// Current configuration options including advanced settings.
    /// </summary>
    public MqttServiceOptions Options { get; } = new();

    /// <inheritdoc />
    protected override void OnSave()
    {
        if (HasErrors)
        {
            Logger?.Log("MQTT create validation failed", LogLevel.Warning);
            return;
        }
        Logger?.Log("MQTT create options start", LogLevel.Debug);
        Options.Host = Host;
        Options.Port = Port;
        Options.ClientId = ClientId;
        Options.Username = string.IsNullOrWhiteSpace(Username) ? null : Username;
        Options.Password = string.IsNullOrWhiteSpace(Password) ? null : Password;
        Options.WillTopic = string.IsNullOrWhiteSpace(Options.WillTopic) ? null : Options.WillTopic;
        Options.WillPayload = string.IsNullOrWhiteSpace(Options.WillPayload) ? null : Options.WillPayload;
        Logger?.Log("MQTT create options finished", LogLevel.Debug);
        RaiseServiceSaved(Options);
    }

    /// <inheritdoc />
    protected override void OnCancel()
    {
        Logger?.Log("MQTT create options cancelled", LogLevel.Debug);
        RaiseEditCancelled();
    }

    /// <inheritdoc />
    protected override void OnAdvancedConfig()
    {
        Logger?.Log("Opening MQTT advanced config", LogLevel.Debug);
        Options.Host = Host;
        Options.Port = Port;
        Options.ClientId = ClientId;
        Options.Username = string.IsNullOrWhiteSpace(Username) ? null : Username;
        Options.Password = string.IsNullOrWhiteSpace(Password) ? null : Password;
        Options.WillTopic = string.IsNullOrWhiteSpace(Options.WillTopic) ? null : Options.WillTopic;
        Options.WillPayload = string.IsNullOrWhiteSpace(Options.WillPayload) ? null : Options.WillPayload;
        RaiseAdvancedConfigRequested(Options);
    }
}
