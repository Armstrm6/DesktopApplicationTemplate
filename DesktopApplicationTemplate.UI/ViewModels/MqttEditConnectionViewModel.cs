using System;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.Extensions.Options;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for editing MQTT connection settings.
/// </summary>
public class MqttEditConnectionViewModel : ValidatableViewModelBase, ILoggingViewModel
{
    private readonly MqttService _service;
    private MqttServiceOptions _options;

    private string _host = string.Empty;
    private int _port;
    private string _clientId = string.Empty;
    private string? _username;
    private string? _password;
    private MqttConnectionType _connectionType;
    private bool _useTls;
    private string? _webSocketPath;
    private bool _isConnected;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttEditConnectionViewModel"/> class.
    /// </summary>
    public MqttEditConnectionViewModel(
        MqttService service,
        IOptions<MqttServiceOptions> options,
        ILoggingService? logger = null)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        Logger = logger;

        Load(_options);

        UpdateCommand = new AsyncRelayCommand(UpdateAsync);
        CancelCommand = new RelayCommand(Cancel);
        ToggleSubscriptionCommand = new AsyncRelayCommand(ToggleSubscriptionAsync);

        _service.ConnectionStateChanged += (_, c) =>
        {
            IsConnected = c;
            OnPropertyChanged(nameof(SubscriptionButtonText));
        };
        IsConnected = _service.IsConnected;
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
            if (_host == value) return;
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
            if (_port == value) return;
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
    /// Connection type (TCP or WebSocket).
    /// </summary>
    public MqttConnectionType ConnectionType
    {
        get => _connectionType;
        set { _connectionType = value; OnPropertyChanged(); UpdateTlsState(); }
    }

    /// <summary>
    /// Whether TLS is used for the connection.
    /// </summary>
    public bool UseTls
    {
        get => _useTls;
        set { _useTls = value; OnPropertyChanged(); }
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
    /// Gets a value indicating whether TLS options are enabled.
    /// </summary>
    public bool IsTlsEnabled => ConnectionType == MqttConnectionType.Tcp;

    /// <summary>
    /// Gets or sets a value indicating whether the service is connected.
    /// </summary>
    public bool IsConnected
    {
        get => _isConnected;
        private set { _isConnected = value; OnPropertyChanged(); OnPropertyChanged(nameof(SubscriptionButtonText)); }
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
        _useTls = _options.UseTls;
        OnPropertyChanged(nameof(Host));
        OnPropertyChanged(nameof(Port));
        OnPropertyChanged(nameof(ClientId));
        OnPropertyChanged(nameof(Username));
        OnPropertyChanged(nameof(Password));
        OnPropertyChanged(nameof(ConnectionType));
        OnPropertyChanged(nameof(WebSocketPath));
        OnPropertyChanged(nameof(UseTls));
        UpdateTlsState();
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
        _options.UseTls = _useTls;
        await _service.ConnectAsync(_options).ConfigureAwait(false);
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
            await _service.DisconnectAsync().ConfigureAwait(false);
            Logger?.Log("MQTT unsubscribe finished", LogLevel.Debug);
        }
        else
        {
            Logger?.Log("MQTT subscribe start", LogLevel.Debug);
            await _service.ConnectAsync().ConfigureAwait(false);
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
            AddError(nameof(Host), "Host required");
        if (string.IsNullOrWhiteSpace(ClientId))
            AddError(nameof(ClientId), "Client Id required");
    }

    private void UpdateTlsState()
    {
        if (_connectionType == MqttConnectionType.WebSocket)
        {
            UseTls = false;
        }
        OnPropertyChanged(nameof(IsTlsEnabled));
    }
}
