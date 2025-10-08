using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;
using MQTTnet.Protocol;

namespace DesktopApplicationTemplate.UI.ViewModels.Mqtt.Create;

/// <summary>
/// View model for configuring a new MQTT service before creation.
/// </summary>
public class MqttCreateServiceViewModel : ServiceCreateViewModelBase<MqttServiceOptions>
{
    private readonly IFileDialogService _fileDialogService;
    private string _host = string.Empty;
    private int _port = 1883;
    private string _clientId = string.Empty;
    private string? _username;
    private string? _password;
    private bool _useClientCertificate;
    private string? _clientCertificatePath;
    private byte[]? _clientCertificate;
    private string? _willTopic;
    private string? _willPayload;
    private MqttQualityOfServiceLevel _willQualityOfService = MqttQualityOfServiceLevel.AtMostOnce;
    private bool _willRetain;
    private int _keepAliveSeconds = 60;
    private bool _cleanSession = true;
    private int _reconnectDelaySeconds;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttCreateServiceViewModel"/> class.
    /// </summary>
    public MqttCreateServiceViewModel(IServiceRule rule, IFileDialogService fileDialogService, ILoggingService? logger = null)
        : base(rule, logger: logger)
    {
        _fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
        BrowseClientCertificateCommand = new RelayCommand(BrowseForClientCertificate);
        ClearClientCertificateCommand = new RelayCommand(ClearClientCertificate);
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
    /// Gets the available MQTT quality of service levels.
    /// </summary>
    public IReadOnlyList<MqttQualityOfServiceLevel> QoSLevels { get; } =
        Enum.GetValues<MqttQualityOfServiceLevel>();

    /// <summary>
    /// Gets a command that opens a file picker for client certificates.
    /// </summary>
    public ICommand BrowseClientCertificateCommand { get; }

    /// <summary>
    /// Gets a command that clears the loaded client certificate.
    /// </summary>
    public ICommand ClearClientCertificateCommand { get; }

    /// <summary>
    /// Gets or sets a value indicating whether a client certificate should be used for TLS authentication.
    /// </summary>
    public bool UseClientCertificate
    {
        get => _useClientCertificate;
        set
        {
            if (_useClientCertificate == value)
            {
                return;
            }

            _useClientCertificate = value;
            if (!value)
            {
                _clientCertificatePath = null;
                _clientCertificate = null;
                ClearErrors(nameof(ClientCertificatePath));
                OnPropertyChanged(nameof(ClientCertificatePath));
            }

            ValidateClientCertificate();
            OnPropertyChanged();
            OnPropertyChanged(nameof(ClientCertificateDisplay));
            OnPropertyChanged(nameof(HasClientCertificate));
        }
    }

    /// <summary>
    /// Gets the selected client certificate path.
    /// </summary>
    public string? ClientCertificatePath
    {
        get => _clientCertificatePath;
        private set
        {
            if (_clientCertificatePath == value)
            {
                return;
            }

            _clientCertificatePath = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ClientCertificateDisplay));
        }
    }

    /// <summary>
    /// Gets display text describing the current certificate selection.
    /// </summary>
    public string ClientCertificateDisplay =>
        !string.IsNullOrWhiteSpace(ClientCertificatePath)
            ? ClientCertificatePath!
            : HasClientCertificate ? "Certificate loaded" : string.Empty;

    /// <summary>
    /// Gets a value indicating whether a certificate is currently loaded.
    /// </summary>
    public bool HasClientCertificate => _clientCertificate is { Length: > 0 };

    /// <summary>
    /// Optional topic for the MQTT will message.
    /// </summary>
    public string? WillTopic
    {
        get => _willTopic;
        set { _willTopic = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Optional payload for the MQTT will message.
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
    /// Gets or sets a value indicating whether the will message should be retained.
    /// </summary>
    public bool WillRetain
    {
        get => _willRetain;
        set { _willRetain = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Keep alive interval in seconds.
    /// </summary>
    public int KeepAliveSeconds
    {
        get => _keepAliveSeconds;
        set
        {
            _keepAliveSeconds = value;
            if (value < 0 || value > ushort.MaxValue)
            {
                AddError(nameof(KeepAliveSeconds), $"Keep alive must be between 0 and {ushort.MaxValue} seconds");
            }
            else
            {
                ClearErrors(nameof(KeepAliveSeconds));
            }

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the client should request a clean session.
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
        set
        {
            _reconnectDelaySeconds = value;
            if (value < 0)
            {
                AddError(nameof(ReconnectDelaySeconds), "Reconnect delay cannot be negative");
            }
            else
            {
                ClearErrors(nameof(ReconnectDelaySeconds));
            }

            OnPropertyChanged();
        }
    }

    /// <inheritdoc />
    protected override void ApplyOptions(MqttServiceOptions options)
    {
        options.Host = Host;
        options.Port = Port;
        options.ClientId = ClientId;
        options.Username = string.IsNullOrWhiteSpace(Username) ? null : Username;
        options.Password = string.IsNullOrWhiteSpace(Password) ? null : Password;
        options.WillTopic = string.IsNullOrWhiteSpace(WillTopic) ? null : WillTopic;
        options.WillPayload = string.IsNullOrWhiteSpace(WillPayload) ? null : WillPayload;
        options.WillQualityOfService = WillQualityOfService;
        options.WillRetain = WillRetain;
        options.KeepAliveSeconds = (ushort)Math.Clamp(KeepAliveSeconds, 0, ushort.MaxValue);
        options.CleanSession = CleanSession;
        options.ReconnectDelay = ReconnectDelaySeconds > 0 ? TimeSpan.FromSeconds(ReconnectDelaySeconds) : null;
        options.ClientCertificate = UseClientCertificate ? _clientCertificate : null;
    }

    /// <inheritdoc />
    protected override void OnAdvancedConfig()
    {
        // Advanced configuration has been folded into the primary editor for MQTT services.
    }

    private void BrowseForClientCertificate()
    {
        var path = _fileDialogService.OpenFile();
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            _clientCertificate = File.ReadAllBytes(path);
            ClientCertificatePath = path;
            UseClientCertificate = true;
            ClearErrors(nameof(ClientCertificatePath));
        }
        catch (Exception)
        {
            _clientCertificate = null;
            ClientCertificatePath = null;
            Logger?.Log("Unable to read MQTT client certificate", LogLevel.Warning);
            AddError(nameof(ClientCertificatePath), "Unable to read certificate file");
        }

        OnPropertyChanged(nameof(HasClientCertificate));
        OnPropertyChanged(nameof(ClientCertificateDisplay));
        ValidateClientCertificate();
    }

    private void ClearClientCertificate()
    {
        _clientCertificate = null;
        ClientCertificatePath = null;
        UseClientCertificate = false;
        ClearErrors(nameof(ClientCertificatePath));
        OnPropertyChanged(nameof(HasClientCertificate));
        OnPropertyChanged(nameof(ClientCertificateDisplay));
    }

    private void ValidateClientCertificate()
    {
        if (!UseClientCertificate)
        {
            ClearErrors(nameof(ClientCertificatePath));
            return;
        }

        if (!HasClientCertificate)
        {
            AddError(nameof(ClientCertificatePath), "Select a client certificate to enable TLS authentication");
        }
        else
        {
            ClearErrors(nameof(ClientCertificatePath));
        }
    }
}
