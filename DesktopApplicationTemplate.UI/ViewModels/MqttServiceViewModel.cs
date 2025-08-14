using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.Options;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    /// <summary>
    /// View model for interacting with an MQTT broker.
    /// </summary>
    public class MqttServiceViewModel : ValidatableViewModelBase, ILoggingViewModel, INetworkAwareViewModel
    {
        private readonly MqttService _service;
        private readonly SaveConfirmationHelper _saveHelper;
        private readonly MqttServiceOptions _options;

        private string _newTopic = string.Empty;
        private string _newEndpoint = string.Empty;
        private string _newMessage = string.Empty;
        private MqttEndpointMessage? _selectedMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttServiceViewModel"/> class.
        /// </summary>
        public MqttServiceViewModel(SaveConfirmationHelper saveHelper, MqttServiceOptions options, MqttService? service = null, ILoggingService? logger = null)
        {
            _saveHelper = saveHelper;
            _options = options;
            Logger = logger;
            _service = service ?? new MqttService(options, logger);
            _service.ConnectionStateChanged += (_, connected) => IsConnected = connected;

            Topics = new ObservableCollection<string>();
            Messages = new ObservableCollection<MqttEndpointMessage>();

            AddTopicCommand = new RelayCommand(AddTopic);
            RemoveTopicCommand = new RelayCommand(RemoveTopic);
            AddMessageCommand = new RelayCommand(AddMessage);
            RemoveMessageCommand = new RelayCommand(RemoveSelectedMessage, () => SelectedMessage != null);
            ConnectCommand = new AsyncRelayCommand(ConnectAsync);
            PublishCommand = new AsyncRelayCommand(PublishSelectedAsync);
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
                if (!InputValidators.IsValidPartialIp(value))
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
                if (int.TryParse(value, out var p) && p >= 0 && p <= 65535)
                {
                    _port = value;
                }
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
        public string Username
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
        public string Password
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
        /// Topics to subscribe to when connected.
        /// </summary>
        public ObservableCollection<string> Topics { get; }

        /// <summary>
        /// Collection of endpoint-message pairs to publish.
        /// </summary>
        public ObservableCollection<MqttEndpointMessage> Messages { get; }

        private bool _useTls;
        public bool UseTls { get => _useTls; set { _useTls = value; OnPropertyChanged(); } }

        private string _newTopic = string.Empty;
        public string NewTopic { get => _newTopic; set { _newTopic = value; OnPropertyChanged(); } }
        /// <summary>
        /// Selected message for publishing.
        /// </summary>
        public MqttEndpointMessage? SelectedMessage
        {
            get => _selectedMessage;
            set { _selectedMessage = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Topic entry used when adding new subscriptions.
        /// </summary>
        public string NewTopic
        {
            get => _newTopic;
            set { _newTopic = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Endpoint used when creating a new message pair.
        /// </summary>
        public string NewEndpoint
        {
            get => _newEndpoint;
            set { _newEndpoint = value; OnPropertyChanged(); }
        }

        public ObservableCollection<EndpointMessagePair> EndpointMessages { get; } = new();
        private EndpointMessagePair? _selectedEndpointMessage;
        public EndpointMessagePair? SelectedEndpointMessage
        {
            get => _selectedEndpointMessage;
            set { _selectedEndpointMessage = value; OnPropertyChanged(); }
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
        public ICommand AddEndpointMessageCommand { get; }
        public ICommand RemoveEndpointMessageCommand { get; }

        private void AddTopic()
        {
            if (!string.IsNullOrWhiteSpace(NewTopic))
            {
                Topics.Add(NewTopic);
                NewTopic = string.Empty;
            }
        }

        private readonly MqttService _service;
        private readonly SaveConfirmationHelper _saveHelper;
        private readonly IDictionary<string, string> _tokenValues;
        private bool _isConnected;

        public MqttServiceViewModel(SaveConfirmationHelper saveHelper, MqttService? service = null, ILoggingService? logger = null, IDictionary<string, string>? tokenValues = null)
        private void RemoveTopic()
        {
            if (Topics.Contains(NewTopic))
                Topics.Remove(NewTopic);
        }

        public MqttServiceViewModel(SaveConfirmationHelper saveHelper, MqttService service, IOptions<MqttServiceOptions> options, ILoggingService logger)
        {
            _saveHelper = saveHelper;
            _service = service;
            Logger = logger;
            _service = service ?? new MqttService(logger);
            _tokenValues = tokenValues ?? new Dictionary<string, string>();
            var opts = options.Value;
            Host = opts.Host;
            Port = opts.Port.ToString();
            ClientId = opts.ClientId;
            Username = opts.Username;
            Password = opts.Password;
            if (opts.Topics != null)
            {
                foreach (var t in opts.Topics)
                    Topics.Add(t);
            }
            AddTopicCommand = new RelayCommand(() => { if(!string.IsNullOrWhiteSpace(NewTopic)){Topics.Add(NewTopic); NewTopic = string.Empty;} });
            RemoveTopicCommand = new RelayCommand(() => { if(Topics.Contains(NewTopic)) Topics.Remove(NewTopic); });
            ConnectCommand = new RelayCommand(async () => await ConnectAsync());
            PublishCommand = new RelayCommand(async () => await PublishAsync());
            SaveCommand = new RelayCommand(Save);
            AddEndpointMessageCommand = new RelayCommand(() =>
            {
                EndpointMessages.Add(new EndpointMessagePair());
                Logger?.Log("Added endpoint-message pair", LogLevel.Debug);
            });
            RemoveEndpointMessageCommand = new RelayCommand(() =>
            {
                if (SelectedEndpointMessage != null)
                {
                    EndpointMessages.Remove(SelectedEndpointMessage);
                    Logger?.Log("Removed endpoint-message pair", LogLevel.Debug);
                }
            });
        }

        private void RemoveSelectedMessage()
        {
            if (SelectedMessage != null)
                Messages.Remove(SelectedMessage);
        }

        /// <summary>
        /// Connects to the broker and subscribes to configured topics.
        /// </summary>
        public async Task ConnectAsync()
        {
            Logger?.Log("MQTT connect start", LogLevel.Debug);
            if (_isConnected)
            {
                await _service.DisconnectAsync();
            }
            await _service.ConnectAsync(Host, int.Parse(Port), ClientId, Username, Password, UseTls);
            await _service.SubscribeAsync(Topics).ConfigureAwait(false);

            var options = new MqttServiceOptions
            {
                Host = Host,
                Port = int.Parse(Port),
                ClientId = ClientId,
                Username = string.IsNullOrWhiteSpace(Username) ? null : Username,
                Password = string.IsNullOrWhiteSpace(Password) ? null : Password
            };
            await _service.ConnectAsync(options);
            await _service.SubscribeAsync(Topics);
            _isConnected = true;
            Logger?.Log("MQTT connected", LogLevel.Debug);
            Logger?.Log("MQTT connect finished", LogLevel.Debug);
        }

        /// <summary>
        /// Publishes the selected endpoint/message pair after resolving tokens.
        /// </summary>
        public async Task PublishSelectedAsync()
        {
            if (SelectedMessage == null)
                return;
            var topic = ResolveTokens(SelectedMessage.Endpoint);
            var payload = ResolveTokens(SelectedMessage.Message);
            Logger?.Log("MQTT publish start", LogLevel.Debug);
            var resolved = ResolveTokens(PublishMessage);
            foreach (var topic in PublishTopic.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                await _service.PublishAsync(topic, resolved);
            }
            Logger?.Log($"Published to {PublishTopic}", LogLevel.Debug);

            Logger?.Log("MQTT publish finished", LogLevel.Debug);
        }

        private string ResolveTokens(string text)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["HOST"] = _options.Host,
                ["PORT"] = _options.Port.ToString(),
                ["CLIENTID"] = _options.ClientId,
                ["USERNAME"] = _options.Username,
                ["PASSWORD"] = _options.Password,
            };

            foreach (var kvp in map)
                text = text.Replace($"{{{kvp.Key}}}", kvp.Value);

            return text;
        }

        private void DisconnectIfConnected()
        {
            if (!_service.IsConnected)
                return;
            Logger?.Log("Disconnecting MQTT due to configuration change", LogLevel.Information);
            try
            {
                _service.DisconnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger?.Log($"MQTT disconnect error: {ex.Message}", LogLevel.Error);
            }
        }

        private void Save() => _saveHelper.Show();

        /// <inheritdoc />
        public void UpdateNetworkConfiguration(NetworkConfiguration configuration)
        {
            Host = configuration.IpAddress;
        }

        private string ResolveTokens(string message)
        {
            foreach (var kvp in _tokenValues)
            {
                message = message.Replace($"{{{kvp.Key}.Message}}", kvp.Value);
            }
            return message;
        }

        // OnPropertyChanged provided by ViewModelBase

    }
}
