using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Runtime.Versioning;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using MQTTnet.Protocol;

namespace DesktopApplicationTemplate.UI.ViewModels.Mqtt;

/// <summary>
/// View model for managing MQTT topic subscriptions and test messages.
/// </summary>
[SupportedOSPlatform("windows")]
public class MqttTagSubscriptionsViewModel : ValidatableViewModelBase, ILoggingViewModel
    {
    private readonly IMqttClientService _clientService;
    private readonly MqttServiceOptions _options;
    private readonly AsyncRelayCommand _addTopicCommand;
    private readonly AsyncRelayCommand _removeTopicCommand;
    private readonly AsyncRelayCommand _connectCommand;
    private readonly AsyncRelayCommand _testConnectionCommand;
    private readonly AsyncRelayCommand<TagSubscription> _sendTestMessageCommand;

    private TagSubscription? _selectedSubscription;
    private string _newTopic = string.Empty;
    private MqttQualityOfServiceLevel _newQoS = MqttQualityOfServiceLevel.AtMostOnce;
    private bool _isConnected;
    private bool _isBusy;
    private ILoggingService? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttTagSubscriptionsViewModel"/> class.
    /// </summary>
    public MqttTagSubscriptionsViewModel(ServiceListModel service, IMqttClientSessionManager sessionManager)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(sessionManager);

        _options = service.MqttOptions ??= new MqttServiceOptions();
        _clientService = sessionManager.GetClient(service);

        Subscriptions = new ObservableCollection<TagSubscription>();
        Subscriptions.CollectionChanged += OnSubscriptionsChanged;
        LogEntries = new ObservableCollection<LogEntry>();

        _addTopicCommand = new AsyncRelayCommand(AddTopicAsync, () => CanAddTopic);
        _removeTopicCommand = new AsyncRelayCommand(RemoveTopicAsync, () => SelectedSubscription != null);
        _connectCommand = new AsyncRelayCommand(ConnectAsync, () => !_isBusy);
        _testConnectionCommand = new AsyncRelayCommand(TestConnectionAsync, () => !_isBusy);
        _sendTestMessageCommand = new AsyncRelayCommand<TagSubscription>(SendTestMessageAsync, CanSendTestMessage);

        _clientService.ConnectionStateChanged += OnConnectionStateChanged;
    }

    /// <inheritdoc />
    public ILoggingService? Logger
    {
        get => _logger;
        set
        {
            if (_logger == value)
                return;

            if (_logger is not null)
                _logger.LogAdded -= OnLogAdded;

            _logger = value;

            if (_logger is not null)
                _logger.LogAdded += OnLogAdded;
        }
    }

    /// <summary>
    /// Gets the current subscriptions.
    /// </summary>
    public ObservableCollection<TagSubscription> Subscriptions { get; }

    /// <summary>
    /// Gets log entries from the logger.
    /// </summary>
    public ObservableCollection<LogEntry> LogEntries { get; }

    /// <summary>
    /// Gets a value indicating whether the service is connected.
    /// </summary>
    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            if (_isConnected == value)
                return;

            _isConnected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ConnectionActionLabel));
            _connectCommand.RaiseCanExecuteChanged();
            _testConnectionCommand.RaiseCanExecuteChanged();
            _sendTestMessageCommand.RaiseCanExecuteChanged();

            if (!value)
            {
                foreach (var subscription in Subscriptions)
                {
                    subscription.IsSubscribed = false;
                    subscription.StatusMessage = "Disconnected";
                }
            }
        }
    }

    /// <summary>
    /// Gets the label for the connection button based on the connection state.
    /// </summary>
    public string ConnectionActionLabel => IsConnected ? "Disconnect" : "Connect";

    /// <summary>
    /// Gets or sets the new topic to subscribe.
    /// </summary>
    public string NewTopic
    {
        get => _newTopic;
        set
        {
            if (_newTopic == value) return;
            _newTopic = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanAddTopic));
            _addTopicCommand.RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Gets or sets the QoS level for new subscriptions.
    /// </summary>
    public MqttQualityOfServiceLevel NewQoS
    {
        get => _newQoS;
        set
        {
            if (_newQoS == value) return;
            _newQoS = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the currently selected subscription.
    /// </summary>
    public TagSubscription? SelectedSubscription
    {
        get => _selectedSubscription;
        set
        {
            if (_selectedSubscription == value) return;
            _selectedSubscription = value;
            OnPropertyChanged();
            _removeTopicCommand.RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Command to add a topic subscription.
    /// </summary>
    public ICommand AddTopicCommand => _addTopicCommand;

    /// <summary>
    /// Command to remove the selected subscription.
    /// </summary>
    public ICommand RemoveTopicCommand => _removeTopicCommand;

    /// <summary>
    /// Command to connect to or disconnect from the MQTT broker.
    /// </summary>
    public ICommand ConnectCommand => _connectCommand;

    /// <summary>
    /// Command to test the MQTT connection using the current settings.
    /// </summary>
    public ICommand TestConnectionCommand => _testConnectionCommand;

    /// <summary>
    /// Command to publish a test message for a subscription.
    /// </summary>
    public ICommand SendTestMessageCommand => _sendTestMessageCommand;

    /// <summary>
    /// Raised when connection settings are invalid and need editing.
    /// </summary>
    public event EventHandler? EditConnectionRequested;

    /// <summary>
    /// Gets a value indicating whether a topic can be added.
    /// </summary>
    public bool CanAddTopic => !string.IsNullOrWhiteSpace(NewTopic);

    private bool TryValidateConnectionOptions(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(_options.Host))
        {
            errors.Add("Host is required.");
        }

        if (_options.Port < 1 || _options.Port > 65535)
        {
            errors.Add("Port must be between 1 and 65535.");
        }

        if (string.IsNullOrWhiteSpace(_options.ClientId))
        {
            errors.Add("Client Id is required.");
        }

        return errors.Count == 0;
    }

    private void NotifyValidationErrors(IEnumerable<string> errors)
    {
        foreach (var error in errors)
        {
            Logger?.Log($"MQTT configuration error: {error}", LogLevel.Warning);
        }

        EditConnectionRequested?.Invoke(this, EventArgs.Empty);
    }

    private async Task AddTopicAsync()
    {
        if (!CanAddTopic)
            return;

        var topic = NewTopic.Trim();
        if (Subscriptions.Any(s => string.Equals(s.Topic, topic, StringComparison.OrdinalIgnoreCase)))
        {
            Logger?.Log($"MQTT topic '{topic}' already exists", LogLevel.Warning);
            return;
        }

        var subscription = new TagSubscription(topic)
        {
            QoS = NewQoS,
            StatusMessage = IsConnected ? "Subscribing..." : "Waiting for connection"
        };

        Subscriptions.Add(subscription);
        NewTopic = string.Empty;

        if (!IsConnected)
            return;

        await SubscribeTopicAsync(subscription);
    }

    private async Task RemoveTopicAsync()
    {
        if (SelectedSubscription is null)
            return;

        try
        {
            await _clientService.UnsubscribeAsync(SelectedSubscription.Topic);
        }
        catch
        {
            // ignore failures, UI already reflects removal
        }

        var topic = SelectedSubscription.Topic;
        foreach (var item in Subscriptions.Where(s => s.Topic == topic).ToList())
            Subscriptions.Remove(item);
        SelectedSubscription = null;
    }

    private bool CanSendTestMessage(TagSubscription? subscription)
        => subscription is not null && IsConnected && !string.IsNullOrWhiteSpace(subscription.OutgoingMessage);

    private async Task SendTestMessageAsync(TagSubscription? subscription)
    {
        if (!CanSendTestMessage(subscription))
            return;

        Logger?.Log("MQTT topic test publish start", LogLevel.Debug);
        await _clientService.PublishAsync(subscription!.Topic, subscription.OutgoingMessage);
        Logger?.Log("MQTT topic test publish finished", LogLevel.Debug);
    }

    public async Task ConnectAsync()
    {
        if (_isBusy)
        {
            return;
        }

        _isBusy = true;
        _connectCommand.RaiseCanExecuteChanged();
        _testConnectionCommand.RaiseCanExecuteChanged();

        try
        {
            if (IsConnected)
            {
                await DisconnectAsync();
                return;
            }

            if (!TryValidateConnectionOptions(out var errors))
            {
                NotifyValidationErrors(errors);
                return;
            }

            await ConnectCoreAsync();
        }
        finally
        {
            _isBusy = false;
            _connectCommand.RaiseCanExecuteChanged();
            _testConnectionCommand.RaiseCanExecuteChanged();
        }
    }

    private async Task<bool> ConnectCoreAsync()
    {
        Logger?.Log("MQTT connect start", LogLevel.Debug);
        try
        {
            await _clientService.ConnectAsync(_options);
            IsConnected = true;
            await SubscribeAllAsync();
            Logger?.Log("MQTT connect finished", LogLevel.Debug);
            return true;
        }
        catch (ArgumentException ex)
        {
            IsConnected = false;
            Logger?.Log(ex.Message, LogLevel.Warning);
            EditConnectionRequested?.Invoke(this, EventArgs.Empty);
            return false;
        }
        catch (Exception ex)
        {
            IsConnected = false;
            Logger?.Log($"MQTT connect failed: {ex.Message}", LogLevel.Error);
            return false;
        }
    }

    private async Task TestConnectionAsync()
    {
        if (_isBusy)
        {
            return;
        }

        if (!TryValidateConnectionOptions(out var errors))
        {
            NotifyValidationErrors(errors);
            return;
        }

        _isBusy = true;
        _connectCommand.RaiseCanExecuteChanged();
        _testConnectionCommand.RaiseCanExecuteChanged();

        try
        {
            if (IsConnected)
            {
                Logger?.Log("MQTT client is already connected.", LogLevel.Information);
                return;
            }

            var connected = await ConnectCoreAsync();
            if (connected)
            {
                Logger?.Log("MQTT test connection succeeded.", LogLevel.Information);
            }
        }
        finally
        {
            _isBusy = false;
            _connectCommand.RaiseCanExecuteChanged();
            _testConnectionCommand.RaiseCanExecuteChanged();
        }
    }

    private async Task SubscribeTopicAsync(TagSubscription subscription)
    {
        if (!IsConnected)
        {
            subscription.IsSubscribed = false;
            subscription.StatusMessage = "Waiting for connection";
            return;
        }

        subscription.StatusMessage = "Subscribing...";

        try
        {
            await _clientService.SubscribeAsync(subscription.Topic, subscription.QoS);
            subscription.IsSubscribed = true;
            subscription.StatusMessage = "Subscribed";
        }
        catch (Exception ex)
        {
            subscription.IsSubscribed = false;
            subscription.StatusMessage = $"Subscribe failed: {ex.Message}";
            Logger?.Log($"MQTT subscribe failed for {subscription.Topic}: {ex.Message}", LogLevel.Error);
        }
    }

    private async Task SubscribeAllAsync()
    {
        if (!IsConnected)
            return;

        foreach (var subscription in Subscriptions)
        {
            await SubscribeTopicAsync(subscription);
        }
    }

    private async Task DisconnectAsync()
    {
        if (!IsConnected)
        {
            return;
        }

        Logger?.Log("MQTT disconnect start", LogLevel.Debug);

        try
        {
            await UnsubscribeAllAsync();
            await _clientService.DisconnectAsync();
            Logger?.Log("MQTT disconnect finished", LogLevel.Debug);
        }
        catch (Exception ex)
        {
            Logger?.Log($"MQTT disconnect failed: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            IsConnected = _clientService.IsConnected;
        }
    }

    private async Task UnsubscribeAllAsync()
    {
        if (!IsConnected)
        {
            foreach (var subscription in Subscriptions)
            {
                subscription.IsSubscribed = false;
                subscription.StatusMessage = "Disconnected";
            }

            return;
        }

        foreach (var subscription in Subscriptions)
        {
            subscription.StatusMessage = "Unsubscribing...";

            try
            {
                await _clientService.UnsubscribeAsync(subscription.Topic);
            }
            catch (Exception ex)
            {
                Logger?.Log($"MQTT unsubscribe failed for {subscription.Topic}: {ex.Message}", LogLevel.Warning);
            }
            finally
            {
                subscription.IsSubscribed = false;
                subscription.StatusMessage = "Disconnected";
            }
        }
    }

    private void OnSubscriptionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems.OfType<TagSubscription>())
            {
                item.PropertyChanged += OnSubscriptionPropertyChanged;
            }
        }

        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems.OfType<TagSubscription>())
            {
                item.PropertyChanged -= OnSubscriptionPropertyChanged;
            }
        }

        _sendTestMessageCommand.RaiseCanExecuteChanged();
    }

    private void OnSubscriptionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not TagSubscription subscription)
            return;

        switch (e.PropertyName)
        {
            case nameof(TagSubscription.OutgoingMessage):
                _sendTestMessageCommand.RaiseCanExecuteChanged();
                break;
            case nameof(TagSubscription.QoS):
                if (IsConnected)
                {
                    _ = SubscribeTopicAsync(subscription);
                }
                else
                {
                    subscription.StatusMessage = "Waiting for connection";
                }

                break;
        }
    }

    private void OnConnectionStateChanged(object? sender, bool connected)
    {
        IsConnected = connected;

        if (connected)
        {
            _ = SubscribeAllAsync();
        }
    }

    private void OnLogAdded(LogEntry entry)
        => LogEntries.Insert(0, entry);
}
