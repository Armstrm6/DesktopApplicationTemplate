using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using Microsoft.Extensions.Options;
using MQTTnet.Protocol;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for interacting with an MQTT broker.
/// </summary>
public class MqttServiceViewModel : ValidatableViewModelBase, ILoggingViewModel, INetworkAwareViewModel
{
    private readonly MqttService _service;
    private readonly IMessageRoutingService _routing;
    private readonly SaveConfirmationHelper _saveHelper;
    private readonly MqttServiceOptions _options;

    private string _newTopic = string.Empty;
    private string? _selectedTopic;
    private string _newEndpoint = string.Empty;
    private string _newMessage = string.Empty;
    private MqttEndpointMessage? _selectedMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttServiceViewModel"/> class.
    /// </summary>
    public MqttServiceViewModel(
        MqttService service,
        IMessageRoutingService routing,
        SaveConfirmationHelper saveHelper,
        IOptions<MqttServiceOptions> options,
        ILoggingService? logger = null)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _routing = routing ?? throw new ArgumentNullException(nameof(routing));
        _saveHelper = saveHelper ?? throw new ArgumentNullException(nameof(saveHelper));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        Logger = logger;

        _service.ConnectionStateChanged += (_, connected) => IsConnected = connected;

        Topics = new ObservableCollection<string>();
        Messages = new ObservableCollection<MqttEndpointMessage>();

        AddTopicCommand = new RelayCommand(AddTopic);
        RemoveTopicCommand = new RelayCommand(RemoveTopic, () => SelectedTopic != null);
        AddMessageCommand = new RelayCommand(AddMessage);
        RemoveMessageCommand = new RelayCommand(RemoveSelectedMessage, () => SelectedMessage != null);
        ConnectCommand = new AsyncRelayCommand(() => ConnectAsync());
        PublishCommand = new AsyncRelayCommand(PublishSelectedAsync, () => SelectedMessage != null);
        SaveCommand = new RelayCommand(Save);
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    private bool _isConnected;
    /// <summary>
    /// Gets a value indicating whether the service is currently connected.
    /// </summary>
    public bool IsConnected
    {
        get => _isConnected;
        private set { _isConnected = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Broker host name or IP address.
    /// </summary>
    public string Host
    {
        get => _options.Host;
        set
        {
            if (_options.Host == value)
                return;
            if (!InputValidators.IsValidHost(value))
            {
                AddError(nameof(Host), "Invalid host");
                Logger?.Log("Invalid MQTT host entered", LogLevel.Warning);
                return;
            }
            ClearErrors(nameof(Host));
            DisconnectIfConnected();
            _options.Host = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Broker port.
    /// </summary>
    public int Port
    {
        get => _options.Port;
        set
        {
            if (_options.Port == value)
                return;
            if (value < 1 || value > 65535)
            {
                AddError(nameof(Port), "Port must be 1-65535");
                Logger?.Log("Invalid MQTT port entered", LogLevel.Warning);
                return;
            }
            ClearErrors(nameof(Port));
            DisconnectIfConnected();
            _options.Port = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Client identifier.
    /// </summary>
    public string ClientId
    {
        get => _options.ClientId;
        set
        {
            if (_options.ClientId == value)
                return;
            if (string.IsNullOrWhiteSpace(value))
            {
                AddError(nameof(ClientId), "ClientId required");
                Logger?.Log("Invalid MQTT client id entered", LogLevel.Warning);
                return;
            }
            ClearErrors(nameof(ClientId));
            DisconnectIfConnected();
            _options.ClientId = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Username for authentication.
    /// </summary>
    public string? Username
    {
        get => _options.Username;
        set
        {
            if (_options.Username == value)
                return;
            DisconnectIfConnected();
            _options.Username = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Password for authentication.
    /// </summary>
    public string? Password
    {
        get => _options.Password;
        set
        {
            if (_options.Password == value)
                return;
            DisconnectIfConnected();
            _options.Password = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// When true, TLS is used for the connection.
    /// </summary>
    public bool UseTls
    {
        get => _options.UseTls;
        set
        {
            if (_options.UseTls == value)
                return;
            DisconnectIfConnected();
            _options.UseTls = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Topic published by the broker when the client disconnects unexpectedly.
    /// </summary>
    public string? WillTopic
    {
        get => _options.WillTopic;
        set
        {
            if (_options.WillTopic == value)
                return;
            if (value is not null && string.IsNullOrWhiteSpace(value))
            {
                AddError(nameof(WillTopic), "Will topic cannot be empty");
                Logger?.Log("Invalid MQTT will topic entered", LogLevel.Warning);
                return;
            }
            ClearErrors(nameof(WillTopic));
            DisconnectIfConnected();
            _options.WillTopic = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Payload published by the broker when the client disconnects unexpectedly.
    /// </summary>
    public string? WillPayload
    {
        get => _options.WillPayload;
        set
        {
            if (_options.WillPayload == value)
                return;
            if (value is not null && string.IsNullOrWhiteSpace(value))
            {
                AddError(nameof(WillPayload), "Will payload cannot be empty");
                Logger?.Log("Invalid MQTT will payload entered", LogLevel.Warning);
                return;
            }
            ClearErrors(nameof(WillPayload));
            DisconnectIfConnected();
            _options.WillPayload = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Quality of service level for the will message.
    /// </summary>
    public MqttQualityOfServiceLevel WillQualityOfService
    {
        get => _options.WillQualityOfService;
        set
        {
            if (_options.WillQualityOfService == value)
                return;
            if (!Enum.IsDefined(typeof(MqttQualityOfServiceLevel), value))
            {
                AddError(nameof(WillQualityOfService), "Invalid QoS");
                Logger?.Log("Invalid MQTT will QoS entered", LogLevel.Warning);
                return;
            }
            ClearErrors(nameof(WillQualityOfService));
            DisconnectIfConnected();
            _options.WillQualityOfService = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// When true, the broker retains the will message.
    /// </summary>
    public bool WillRetain
    {
        get => _options.WillRetain;
        set
        {
            if (_options.WillRetain == value)
                return;
            DisconnectIfConnected();
            _options.WillRetain = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Keep alive interval in seconds.
    /// </summary>
    public int KeepAliveSeconds
    {
        get => _options.KeepAliveSeconds;
        set
        {
            if (_options.KeepAliveSeconds == value)
                return;
            if (value < 0 || value > ushort.MaxValue)
            {
                AddError(nameof(KeepAliveSeconds), "Keep alive must be 0-65535");
                Logger?.Log("Invalid MQTT keep alive entered", LogLevel.Warning);
                return;
            }
            ClearErrors(nameof(KeepAliveSeconds));
            DisconnectIfConnected();
            _options.KeepAliveSeconds = (ushort)value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Whether the client requests a clean session on connect.
    /// </summary>
    public bool CleanSession
    {
        get => _options.CleanSession;
        set
        {
            if (_options.CleanSession == value)
                return;
            DisconnectIfConnected();
            _options.CleanSession = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Delay before attempting to reconnect after a disconnect.
    /// </summary>
    public int? ReconnectDelay
    {
        get => _options.ReconnectDelay.HasValue ? (int?)_options.ReconnectDelay.Value.TotalSeconds : null;
        set
        {
            var current = _options.ReconnectDelay.HasValue ? (int?)_options.ReconnectDelay.Value.TotalSeconds : null;
            if (current == value)
                return;
            if (value < 0)
            {
                AddError(nameof(ReconnectDelay), "Reconnect delay must be >= 0");
                Logger?.Log("Invalid MQTT reconnect delay entered", LogLevel.Warning);
                return;
            }
            ClearErrors(nameof(ReconnectDelay));
            DisconnectIfConnected();
            _options.ReconnectDelay = value.HasValue ? TimeSpan.FromSeconds(value.Value) : null;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Topics to subscribe to when connected.
    /// </summary>
    public ObservableCollection<string> Topics { get; }

    /// <summary>
    /// Topic entry used when adding new subscriptions.
    /// </summary>
    public string NewTopic
    {
        get => _newTopic;
        set { _newTopic = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Currently selected topic.
    /// </summary>
    public string? SelectedTopic
    {
        get => _selectedTopic;
        set { _selectedTopic = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Collection of endpoint-message pairs to publish.
    /// </summary>
    public ObservableCollection<MqttEndpointMessage> Messages { get; }

    /// <summary>
    /// Endpoint used when creating a new message pair.
    /// </summary>
    public string NewEndpoint
    {
        get => _newEndpoint;
        set { _newEndpoint = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Message used when creating a new message pair.
    /// </summary>
    public string NewMessage
    {
        get => _newMessage;
        set { _newMessage = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Selected message for publishing.
    /// </summary>
    public MqttEndpointMessage? SelectedMessage
    {
        get => _selectedMessage;
        set { _selectedMessage = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Command to add a topic subscription.
    /// </summary>
    public ICommand AddTopicCommand { get; }

    /// <summary>
    /// Command to remove a topic subscription.
    /// </summary>
    public ICommand RemoveTopicCommand { get; }

    /// <summary>
    /// Command to add a new endpoint/message pair.
    /// </summary>
    public ICommand AddMessageCommand { get; }

    /// <summary>
    /// Command to remove the selected endpoint/message pair.
    /// </summary>
    public ICommand RemoveMessageCommand { get; }

    /// <summary>
    /// Command to connect to the broker.
    /// </summary>
        public ICommand ConnectCommand { get; }

    /// <summary>
    /// Command to publish the selected message.
    /// </summary>
        public ICommand PublishCommand { get; }

    /// <summary>
    /// Command to trigger save confirmation.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Raised when connection settings require editing.
    /// </summary>
    public event EventHandler? EditConnectionRequested;

    private void AddTopic()
    {
        if (string.IsNullOrWhiteSpace(NewTopic))
            return;
        Topics.Add(NewTopic);
        NewTopic = string.Empty;
    }

    private void RemoveTopic()
    {
        if (SelectedTopic is null)
            return;
        Topics.Remove(SelectedTopic);
        SelectedTopic = null;
    }

    private void AddMessage()
    {
        if (string.IsNullOrWhiteSpace(NewEndpoint) || string.IsNullOrWhiteSpace(NewMessage))
            return;
        Messages.Add(new MqttEndpointMessage { Endpoint = NewEndpoint, Message = NewMessage });
        NewEndpoint = string.Empty;
        NewMessage = string.Empty;
    }

    private void RemoveSelectedMessage()
    {
        if (SelectedMessage is null)
            return;
        Messages.Remove(SelectedMessage);
        SelectedMessage = null;
    }

    /// <summary>
    /// Connects to the broker.
    /// </summary>
        public async Task ConnectAsync(MqttServiceOptions? options = null)
    {
        Logger?.Log("MQTT connect start", LogLevel.Debug);
        try
        {
            await _service.ConnectAsync(options).ConfigureAwait(false);
            IsConnected = true;
            Logger?.Log("MQTT connect finished", LogLevel.Debug);
        }
        catch (ArgumentException ex)
        {
            Logger?.Log(ex.Message, LogLevel.Warning);
            EditConnectionRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Publishes the selected endpoint/message pair after resolving tokens.
    /// </summary>
    public async Task PublishSelectedAsync()
    {
        if (SelectedMessage is null)
            return;
        Logger?.Log("MQTT publish start", LogLevel.Debug);
        var topic = _routing.ResolveTokens(SelectedMessage.Endpoint);
        await _service.PublishAsync(topic, SelectedMessage.Message).ConfigureAwait(false);
        Logger?.Log("MQTT publish finished", LogLevel.Debug);
    }

    private void DisconnectIfConnected()
    {
        if (!_service.IsConnected)
            return;
        Logger?.Log("Disconnecting MQTT due to configuration change", LogLevel.Debug);
        _ = _service.DisconnectAsync();
    }

    private void Save() => _saveHelper.Show();

    /// <inheritdoc />
    public void UpdateNetworkConfiguration(NetworkConfiguration configuration)
    {
        Host = configuration.IpAddress;
    }
}

