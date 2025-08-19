using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;


using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using MQTTnet.Protocol;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for managing MQTT tag subscriptions and test messages.
/// </summary>
public class MqttTagSubscriptionsViewModel : ValidatableViewModelBase, ILoggingViewModel
{
    private readonly MqttService _service;
    private readonly AsyncRelayCommand<TagSubscription> _testTagEndpointCommand;

    private string _newTag = string.Empty;
    private TagSubscription? _selectedSubscription;
    private string _newTopic = string.Empty;
    private TagSubscription? _selectedSubscription;
    private string _testMessage = string.Empty;

    private MqttQualityOfServiceLevel _newQoS = MqttQualityOfServiceLevel.AtMostOnce;

    private bool _isConnected;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttTagSubscriptionsViewModel"/> class.
    /// </summary>
    public MqttTagSubscriptionsViewModel(MqttService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));

        Subscriptions = new ObservableCollection<TagSubscription>();
        Subscriptions.CollectionChanged += OnSubscriptionsChanged;

        AddTagCommand = new RelayCommand(AddTag);
        RemoveTagCommand = new RelayCommand(RemoveTag, () => SelectedSubscription != null);

        Subscriptions = new ObservableCollection<TagSubscription>(_service.TagSubscriptions);
        _service.TagSubscriptionChanged += OnTagSubscriptionChanged;

        AddTopicCommand = new RelayCommand(AddTopic);
        RemoveTopicCommand = new RelayCommand(RemoveTopic, () => SelectedSubscription != null);
        TagSubscriptions = new ObservableCollection<TagSubscription>();
        AddTopicCommand = new RelayCommand(AddTopic);
        RemoveTopicCommand = new RelayCommand(RemoveTopic, () => SelectedSubscription != null);
        Topics = new ObservableCollection<TagSubscription>();
        AddTopicCommand = new RelayCommand(AddTopic);
        RemoveTopicCommand = new RelayCommand(RemoveTopic, () => SelectedTopic != null);
        Subscriptions = new ObservableCollection<TagSubscription>();
        SubscriptionResults = new ObservableCollection<SubscriptionResult>();

        AddTopicCommand = new AsyncRelayCommand(AddTopicAsync);
        RemoveTopicCommand = new AsyncRelayCommand(RemoveTopicAsync, () => SelectedSubscription != null);
        ConnectCommand = new AsyncRelayCommand(ConnectAsync);
        _testTagEndpointCommand = new AsyncRelayCommand<TagSubscription>(TestTagEndpointAsync, CanTestTagEndpoint);
    }

    private void OnSubscriptionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (TagSubscription sub in e.NewItems)
            {
                sub.PropertyChanged += OnSubscriptionPropertyChanged;
            }
        }
        if (e.OldItems != null)
        {
            foreach (TagSubscription sub in e.OldItems)
            {
                sub.PropertyChanged -= OnSubscriptionPropertyChanged;
            }
        }
        _testTagEndpointCommand.RaiseCanExecuteChanged();
    }

    private void OnSubscriptionPropertyChanged(object? sender, PropertyChangedEventArgs e)
        => _testTagEndpointCommand.RaiseCanExecuteChanged();

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Tag subscriptions managed by this service.
    /// </summary>
    public ObservableCollection<TagSubscription> Subscriptions { get; }
    /// Tag subscriptions tracked by this service.
    /// </summary>
    public ObservableCollection<TagSubscription> Subscriptions { get; }
    /// Subscriptions maintained by this service.
    /// </summary>
    public ObservableCollection<TagSubscription> TagSubscriptions { get; }
    public ObservableCollection<TagSubscription> Topics { get; }
    public ObservableCollection<TagSubscription> Subscriptions { get; }

    /// <summary>
    /// Results of subscription attempts for UI feedback.
    /// </summary>
    public ObservableCollection<SubscriptionResult> SubscriptionResults { get; }

    /// <summary>
    /// Gets or sets the new tag entry.
    /// </summary>
    public string NewTag
    {
        get => _newTag;
        set { _newTag = value; OnPropertyChanged(); }
        get => _newTopic;
        set { _newTopic = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Gets or sets the selected tag subscription.
    /// </summary>
    public TagSubscription? SelectedSubscription
    {
        get => _selectedSubscription;
        set
        {
    /// Gets or sets the selected subscription.
    /// </summary>
    /// Gets or sets the QoS level for new subscriptions.
    /// </summary>
    public MqttQualityOfServiceLevel NewQoS
    {
        get => _newQoS;
        set { _newQoS = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Gets or sets the selected subscription.
    /// </summary>

    public TagSubscription? SelectedTopic
    public TagSubscription? SelectedSubscription
    {
        get => _selectedSubscription;
        set
        {
            _selectedSubscription = value;
            OnPropertyChanged();
            ((RelayCommand)RemoveTagCommand).RaiseCanExecuteChanged();
            if (_selectedSubscription != null)
            {
                _selectedSubscription.PropertyChanged -= SelectedSubscription_PropertyChanged;
            }
            _selectedSubscription = value;
            if (_selectedSubscription != null)
            {
                _selectedSubscription.PropertyChanged += SelectedSubscription_PropertyChanged;
            }
            if (_selectedSubscription == value) return;
            if (_selectedSubscription is not null)
                _selectedSubscription.PropertyChanged -= SelectedSubscriptionOnPropertyChanged;
            _selectedSubscription = value;
            OnPropertyChanged();
            ((AsyncRelayCommand)RemoveTopicCommand).RaiseCanExecuteChanged();
            ((AsyncRelayCommand)PublishTestMessageCommand).RaiseCanExecuteChanged();
            if (_selectedSubscription is not null)
                _selectedSubscription.PropertyChanged += SelectedSubscriptionOnPropertyChanged;
        }
    }


    private void SelectedSubscriptionOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TagSubscription.OutgoingMessage))
        {
            ((AsyncRelayCommand)PublishTestMessageCommand).RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Command to add a tag subscription.
    /// </summary>
    public ICommand AddTagCommand { get; }

    /// <summary>
    /// Command to remove the selected subscription.
    /// </summary>
    public ICommand RemoveTagCommand { get; }

    /// <summary>
    /// Command to connect to the broker.
    /// </summary>
    public ICommand ConnectCommand { get; }

    /// <summary>
    /// Command to test publishing to a tag's endpoint.
    /// </summary>
    public ICommand TestTagEndpointCommand => _testTagEndpointCommand;

    private void AddTag()
    /// <summary>
    /// Attempts to subscribe to the specified topic and records the result.
    /// </summary>
    public async Task AddTopicAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTag))
            return;
        var sub = new TagSubscription { Tag = NewTag };
        Subscriptions.Add(sub);
        NewTag = string.Empty;
    }

    private void RemoveTag()
    {
        if (SelectedSubscription is null)
            return;
        _service.UpdateTagSubscription(new TagSubscription(NewTopic));
        TagSubscriptions.Add(new TagSubscription { Tag = NewTopic });
        Topics.Add(new TagSubscription { Topic = NewTopic, QoS = MqttQualityOfServiceLevel.AtMostOnce });
        NewTopic = string.Empty;

        try
        {
            await _service.SubscribeAsync(NewTopic, NewQoS).ConfigureAwait(false);
            Subscriptions.Add(new TagSubscription { Topic = NewTopic, QoS = NewQoS });
            SubscriptionResults.Add(new SubscriptionResult(NewTopic, true, $"Subscribed to {NewTopic}"));
            NewTopic = string.Empty;
        }
        catch (Exception ex)
        {
            SubscriptionResults.Add(new SubscriptionResult(NewTopic, false, ex.Message));
        }
    }

    /// <summary>
    /// Removes the selected subscription and unsubscribes from the broker.
    /// </summary>
    public async Task RemoveTopicAsync()
    {
        if (SelectedSubscription is null)
            return;

        TagSubscriptions.Remove(SelectedSubscription);

        try
        {
            await _service.UnsubscribeAsync(SelectedSubscription.Topic).ConfigureAwait(false);
        }
        catch
        {
            // ignore unsubscribe failures; UI already reflects removal
        }
        Subscriptions.Remove(SelectedSubscription);
        SelectedSubscription = null;
    }

    private bool CanTestTagEndpoint(TagSubscription? sub)
        => sub is not null && !string.IsNullOrWhiteSpace(sub.Endpoint) && !string.IsNullOrWhiteSpace(sub.OutgoingMessage);

    private async Task TestTagEndpointAsync(TagSubscription? sub)
    {
        if (!CanTestTagEndpoint(sub))
            return;
        Logger?.Log("MQTT tag test publish start", LogLevel.Debug);
        await _service.PublishAsync(sub!.Endpoint, sub.OutgoingMessage).ConfigureAwait(false);
        Logger?.Log("MQTT tag test publish finished", LogLevel.Debug);
    }
    private bool CanPublishTest() => SelectedSubscription != null && !string.IsNullOrWhiteSpace(TestMessage);
    private bool CanPublishTest() => SelectedSubscription != null && !string.IsNullOrWhiteSpace(SelectedSubscription.OutgoingMessage);

    /// <summary>
    /// Connects to the MQTT broker.
    /// </summary>
    public async Task ConnectAsync()
    {
        Logger?.Log("MQTT connect start", LogLevel.Debug);
        await _service.ConnectAsync().ConfigureAwait(false);
        Logger?.Log("MQTT connect finished", LogLevel.Debug);
    }
        foreach (var topic in Topics)
        {
            await _service.SubscribeAsync(topic.Topic, topic.QoS).ConfigureAwait(false);
        }
        IsConnected = true;
        Logger?.Log("MQTT connect finished", LogLevel.Debug);
    }

    /// <summary>
    /// Publishes the test message to the selected topic.
    /// </summary>
    public async Task PublishTestAsync()
    {
        if (!CanPublishTest())
            return;
        Logger?.Log("MQTT test publish start", LogLevel.Debug);
        await _service.PublishAsync(SelectedSubscription!.Topic, TestMessage).ConfigureAwait(false);
        Logger?.Log("MQTT test publish finished", LogLevel.Debug);
    }

    private void OnTagSubscriptionChanged(object? sender, TagSubscription subscription)
    {
        var existing = Subscriptions.FirstOrDefault(t => t.Topic == subscription.Topic);
        if (existing is null)
        {
            Subscriptions.Add(subscription);
        }
        else
        {
            existing.StatusColor = subscription.StatusColor;
            existing.Icon = subscription.Icon;
        await _service.PublishAsync(SelectedSubscription!.Tag, SelectedSubscription.OutgoingMessage).ConfigureAwait(false);
        await _service.PublishAsync(SelectedTopic!.Topic, TestMessage).ConfigureAwait(false);
        await _service.PublishAsync(SelectedSubscription!.Topic, SelectedSubscription.OutgoingMessage).ConfigureAwait(false);
        Logger?.Log("MQTT test publish finished", LogLevel.Debug);
    }

    private void SelectedSubscription_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TagSubscription.OutgoingMessage))
        {
            ((AsyncRelayCommand)PublishTestMessageCommand).RaiseCanExecuteChanged();
        }
    }
}

