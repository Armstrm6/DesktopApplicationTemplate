using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using MQTTnet.Protocol;

namespace DesktopApplicationTemplate.UI.ViewModels.Mqtt.Edit;

/// <summary>
/// View model for editing MQTT connection settings.
/// </summary>
public class MqttEditConnectionViewModel : ValidatableViewModelBase, ILoggingViewModel
{
    private readonly IMqttClientService _clientService;
    private readonly IFileDialogService _fileDialogService;
    private MqttServiceOptions _options;

    private string _host = string.Empty;
    private int _port;
    private string _clientId = string.Empty;
    private string? _username;
    private string? _password;
    private MqttConnectionType _connectionType;
    private string? _webSocketPath;
    private bool _useClientCertificate;
    private string? _clientCertificatePath;
    private byte[]? _clientCertificate;
    private string? _willTopic;
    private string? _willPayload;
    private MqttQualityOfServiceLevel _willQualityOfService;
    private bool _willRetain;
    private int _keepAliveSeconds = 60;
    private bool _cleanSession = true;
    private int _reconnectDelaySeconds;
    private bool _isConnected;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttEditConnectionViewModel"/> class.
    /// </summary>
    public MqttEditConnectionViewModel(
        ServiceListModel service,
        IMqttClientSessionManager sessionManager,
        IFileDialogService fileDialogService,
        ILoggingService? logger = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(sessionManager);

        _clientService = sessionManager.GetClient(service);
        _options = service.GetOptions<MqttServiceOptions>() ?? new MqttServiceOptions();
        service.SetOptions(_options);
        _fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
        Logger = logger;

        BrowseClientCertificateCommand = new RelayCommand(BrowseForClientCertificate);
        ClearClientCertificateCommand = new RelayCommand(ClearClientCertificate);

        UpdateCommand = new AsyncRelayCommand(UpdateAsync);
        CancelCommand = new RelayCommand(Cancel);
        ToggleSubscriptionCommand = new AsyncRelayCommand(ToggleSubscriptionAsync);

        Load(_options);

        _clientService.ConnectionStateChanged += (_, c) =>
        {
            IsConnected = c;
            OnPropertyChanged(nameof(SubscriptionButtonText));
        };
        IsConnected = _clientService.IsConnected;
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Raised when the view requests to close.
    /// </summary>
    public event EventHandler? RequestClose;

    /// <summary>
    /// MQTT broker host name or IP.
    /// </summary>
    public string Host
    {
        get => _host;
        set
        {
            if (_host == value)
            {
                return;
            }

            if (!InputValidators.IsValidHost(value))
            {
                AddError(nameof(Host), "Invalid host");
                Logger?.Log("Invalid MQTT host entered", LogLevel.Warning);
                return;
            }

            ClearErrors(nameof(Host));
            _host = value;
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
            if (_port == value)
            {
                return;
            }

            if (value < 1 || value > 65535)
            {
                AddError(nameof(Port), "Port must be 1-65535");
                Logger?.Log("Invalid MQTT port entered", LogLevel.Warning);
                return;
            }

            ClearErrors(nameof(Port));
            _port = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Client identifier.
    /// </summary>
    public string ClientId
    {
        get => _clientId;
        set { _clientId = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Optional username.
    /// </summary>
    public string? Username
    {
        get => _username;
        set { _username = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Optional password.
    /// </summary>
    public string? Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Connection type.
    /// </summary>
    public MqttConnectionType ConnectionType
    {
        get => _connectionType;
        set { _connectionType = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// WebSocket path when using a WebSocket connection.
    /// </summary>
    public string? WebSocketPath
    {
        get => _webSocketPath;
        set { _webSocketPath = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Available MQTT quality of service levels.
    /// </summary>
    public IReadOnlyList<MqttQualityOfServiceLevel> QoSLevels { get; } = Enum.GetValues<MqttQualityOfServiceLevel>();

    /// <summary>
    /// Gets a command that opens the file picker for selecting a TLS client certificate.
    /// </summary>
    public ICommand BrowseClientCertificateCommand { get; }

    /// <summary>
    /// Gets a command that clears any loaded TLS client certificate.
    /// </summary>
    public ICommand ClearClientCertificateCommand { get; }

    /// <summary>
    /// Gets or sets a value indicating whether a client certificate is used for TLS authentication.
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
                OnPropertyChanged(nameof(ClientCertificatePath));
                ClearErrors(nameof(ClientCertificatePath));
            }

            ValidateClientCertificate();
            OnPropertyChanged();
            OnPropertyChanged(nameof(ClientCertificateDisplay));
            OnPropertyChanged(nameof(HasClientCertificate));
        }
    }

    /// <summary>
    /// Gets the path to the loaded TLS client certificate.
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
    /// Gets a value indicating whether certificate data has been loaded.
    /// </summary>
    public bool HasClientCertificate => _clientCertificate is { Length: > 0 };

    /// <summary>
    /// Optional will topic.
    /// </summary>
    public string? WillTopic
    {
        get => _willTopic;
        set { _willTopic = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Optional will payload.
    /// </summary>
    public string? WillPayload
    {
        get => _willPayload;
        set { _willPayload = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Will message QoS.
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
    /// Gets or sets a value indicating whether to request a clean session.
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

    /// <summary>
    /// Gets or sets a value indicating whether the service is connected.
    /// </summary>
    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            _isConnected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SubscriptionButtonText));
        }
    }

    /// <summary>
    /// Text for the subscription toggle button.
    /// </summary>
    public string SubscriptionButtonText => IsConnected ? "Unsubscribe" : "Subscribe";

    /// <summary>
    /// Command to update the connection settings.
    /// </summary>
    public ICommand UpdateCommand { get; }

    /// <summary>
    /// Command to cancel editing.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Command to unsubscribe from the broker.
    /// </summary>
    public ICommand ToggleSubscriptionCommand { get; }

    internal void Load(MqttServiceOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _host = _options.Host;
        _port = _options.Port;
        _clientId = _options.ClientId;
        _username = _options.Username;
        _password = _options.Password;
        _connectionType = _options.ConnectionType;
        _webSocketPath = _options.WebSocketPath;
        _clientCertificate = _options.ClientCertificate;
        _useClientCertificate = _clientCertificate is { Length: > 0 };
        _clientCertificatePath = null;
        _willTopic = _options.WillTopic;
        _willPayload = _options.WillPayload;
        _willQualityOfService = _options.WillQualityOfService;
        _willRetain = _options.WillRetain;
        _keepAliveSeconds = _options.KeepAliveSeconds;
        _cleanSession = _options.CleanSession;
        _reconnectDelaySeconds = _options.ReconnectDelay?.Seconds ?? 0;

        OnPropertyChanged(nameof(Host));
        OnPropertyChanged(nameof(Port));
        OnPropertyChanged(nameof(ClientId));
        OnPropertyChanged(nameof(Username));
        OnPropertyChanged(nameof(Password));
        OnPropertyChanged(nameof(ConnectionType));
        OnPropertyChanged(nameof(WebSocketPath));
        OnPropertyChanged(nameof(UseClientCertificate));
        OnPropertyChanged(nameof(ClientCertificatePath));
        OnPropertyChanged(nameof(ClientCertificateDisplay));
        OnPropertyChanged(nameof(HasClientCertificate));
        OnPropertyChanged(nameof(WillTopic));
        OnPropertyChanged(nameof(WillPayload));
        OnPropertyChanged(nameof(WillQualityOfService));
        OnPropertyChanged(nameof(WillRetain));
        OnPropertyChanged(nameof(KeepAliveSeconds));
        OnPropertyChanged(nameof(CleanSession));
        OnPropertyChanged(nameof(ReconnectDelaySeconds));

        ValidateClientCertificate();
    }

    /// <summary>
    /// Applies changes and reconnects using updated options.
    /// </summary>
    public async Task UpdateAsync()
    {
        Logger?.Log("MQTT connection update start", LogLevel.Debug);
        _options.Host = _host;
        _options.Port = _port;
        _options.ClientId = _clientId;
        _options.Username = _username;
        _options.Password = _password;
        _options.ConnectionType = _connectionType;
        _options.WebSocketPath = _webSocketPath;
        _options.WillTopic = string.IsNullOrWhiteSpace(_willTopic) ? null : _willTopic;
        _options.WillPayload = string.IsNullOrWhiteSpace(_willPayload) ? null : _willPayload;
        _options.WillQualityOfService = _willQualityOfService;
        _options.WillRetain = _willRetain;
        _options.KeepAliveSeconds = (ushort)Math.Clamp(_keepAliveSeconds, 0, ushort.MaxValue);
        _options.CleanSession = _cleanSession;
        _options.ReconnectDelay = _reconnectDelaySeconds > 0 ? TimeSpan.FromSeconds(_reconnectDelaySeconds) : null;

        if (UseClientCertificate)
        {
            if (!string.IsNullOrWhiteSpace(ClientCertificatePath))
            {
                try
                {
                    _clientCertificate = File.ReadAllBytes(ClientCertificatePath);
                }
                catch (Exception)
                {
                    AddError(nameof(ClientCertificatePath), "Unable to read certificate file");
                    Logger?.Log("Failed to load MQTT client certificate", LogLevel.Warning);
                    return;
                }
            }

            if (_clientCertificate is not { Length: > 0 })
            {
                AddError(nameof(ClientCertificatePath), "Select a client certificate to enable TLS authentication");
                Logger?.Log("Client certificate required for TLS connection", LogLevel.Warning);
                return;
            }

            _options.ClientCertificate = _clientCertificate;
        }
        else
        {
            _options.ClientCertificate = null;
        }

        await _clientService.ConnectAsync(_options).ConfigureAwait(false);
        Logger?.Log("MQTT connection update finished", LogLevel.Debug);
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Cancels editing without applying changes.
    /// </summary>
    public void Cancel()
    {
        Logger?.Log("MQTT connection update canceled", LogLevel.Debug);
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Disconnects from the broker.
    /// </summary>
    public async Task ToggleSubscriptionAsync()
    {
        if (IsConnected)
        {
            Logger?.Log("MQTT unsubscribe start", LogLevel.Debug);
            await _clientService.DisconnectAsync().ConfigureAwait(false);
            Logger?.Log("MQTT unsubscribe finished", LogLevel.Debug);
        }
        else
        {
            Logger?.Log("MQTT subscribe start", LogLevel.Debug);
            await _clientService.ConnectAsync().ConfigureAwait(false);
            Logger?.Log("MQTT subscribe finished", LogLevel.Debug);
        }

        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Highlights missing required fields.
    /// </summary>
    public void HighlightMissingFields()
    {
        if (string.IsNullOrWhiteSpace(Host))
        {
            AddError(nameof(Host), "Host required");
        }

        if (string.IsNullOrWhiteSpace(ClientId))
        {
            AddError(nameof(ClientId), "Client Id required");
        }

        if (UseClientCertificate && !HasClientCertificate)
        {
            AddError(nameof(ClientCertificatePath), "Select a client certificate to enable TLS authentication");
        }
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
            AddError(nameof(ClientCertificatePath), "Unable to read certificate file");
            _clientCertificate = null;
            ClientCertificatePath = null;
            Logger?.Log("Unable to read MQTT client certificate", LogLevel.Warning);
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

        if (_clientCertificate is { Length: > 0 })
        {
            ClearErrors(nameof(ClientCertificatePath));
        }
        else
        {
            AddError(nameof(ClientCertificatePath), "Select a client certificate to enable TLS authentication");
        }
    }
}
