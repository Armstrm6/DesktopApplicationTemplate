using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using MQTTnet.Protocol;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for managing MQTT topic subscriptions and test messages.
/// </summary>
public class MqttTagSubscriptionsViewModel : ValidatableViewModelBase, ILoggingViewModel
{
    private readonly MqttService _service;
    private readonly AsyncRelayCommand _addTopicCommand;
    private readonly AsyncRelayCommand _removeTopicCommand;
    private readonly AsyncRelayCommand _publishTestMessageCommand;
    private readonly AsyncRelayCommand<TagSubscription> _testTagEndpointCommand;

    private TagSubscription? _selectedSubscription;
    private string _newTopic = string.Empty;
    private MqttQualityOfServiceLevel _newQoS = MqttQualityOfServiceLevel.AtMostOnce;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttTagSubscriptionsViewModel"/> class.
    /// </summary>
    public MqttTagSubscriptionsViewModel(MqttService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));

        Subscriptions = new ObservableCollection<TagSubscription>(_service.TagSubscriptions);
        SubscriptionResults = new ObservableCollection<SubscriptionResult>();
        _service.TagSubscriptionChanged += OnTagSubscriptionChanged;

        _addTopicCommand = new AsyncRelayCommand(AddTopicAsync, CanAddTopic);
        _removeTopicCommand = new AsyncRelayCommand(RemoveTopicAsync, () => SelectedSubscription != null);
        _publishTestMessageCommand = new AsyncRelayCommand(PublishTestMessageAsync, CanPublishTestMessage);
        ConnectCommand = new AsyncRelayCommand(ConnectAsync);
        _testTagEndpointCommand = new AsyncRelayCommand<TagSubscription>(TestTagEndpointAsync, CanTestTagEndpoint);
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Gets the current subscriptions.
    /// </summary>
    public ObservableCollection<TagSubscription> Subscriptions { get; }

    /// <summary>
    /// Gets the results of subscription attempts for UI feedback.
    /// </summary>
    public ObservableCollection<SubscriptionResult> SubscriptionResults { get; }

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

            if (_selectedSubscription is not null)
            {
                _selectedSubscription.PropertyChanged -= SelectedSubscriptionOnPropertyChanged;
            }

            _selectedSubscription = value;
            OnPropertyChanged();
            _removeTopicCommand.RaiseCanExecuteChanged();
            _publishTestMessageCommand.RaiseCanExecuteChanged();

            if (_selectedSubscription is not null)
            {
                _selectedSubscription.PropertyChanged += SelectedSubscriptionOnPropertyChanged;
            }
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
    /// Command to publish the selected subscription's test message.
    /// </summary>
    public ICommand PublishTestMessageCommand => _publishTestMessageCommand;

    /// <summary>
    /// Command to connect to the MQTT broker.
    /// </summary>
    public ICommand ConnectCommand { get; }

    /// <summary>
    /// Command to test publishing to a tag's endpoint.
    /// </summary>
    public ICommand TestTagEndpointCommand => _testTagEndpointCommand;

    private bool CanAddTopic() => !string.IsNullOrWhiteSpace(NewTopic);

    private async Task AddTopicAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTopic))
            return;

        try
        {
            await _service.SubscribeAsync(NewTopic, NewQoS).ConfigureAwait(false);
            Subscriptions.Add(new TagSubscription(NewTopic) { QoS = NewQoS });
            SubscriptionResults.Add(new SubscriptionResult(NewTopic, true, $"Subscribed to {NewTopic}"));
            NewTopic = string.Empty;
        }
        catch (Exception ex)
        {
            SubscriptionResults.Add(new SubscriptionResult(NewTopic, false, ex.Message));
        }
    }

    private async Task RemoveTopicAsync()
    {
        if (SelectedSubscription is null)
            return;

        try
        {
            await _service.UnsubscribeAsync(SelectedSubscription.Topic).ConfigureAwait(false);
        }
        catch
        {
            // ignore failures, UI already reflects removal
        }

        Subscriptions.Remove(SelectedSubscription);
        SelectedSubscription = null;
    }

    private bool CanPublishTestMessage()
        => SelectedSubscription is not null && !string.IsNullOrWhiteSpace(SelectedSubscription.OutgoingMessage);

    private async Task PublishTestMessageAsync()
    {
        if (!CanPublishTestMessage())
            return;

        Logger?.Log("MQTT test publish start", LogLevel.Debug);
        await _service.PublishAsync(SelectedSubscription!.Topic, SelectedSubscription.OutgoingMessage).ConfigureAwait(false);
        Logger?.Log("MQTT test publish finished", LogLevel.Debug);
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

    private async Task ConnectAsync()
    {
        Logger?.Log("MQTT connect start", LogLevel.Debug);
        await _service.ConnectAsync().ConfigureAwait(false);
        Logger?.Log("MQTT connect finished", LogLevel.Debug);
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
        }
    }

    private void SelectedSubscriptionOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TagSubscription.OutgoingMessage))
        {
            _publishTestMessageCommand.RaiseCanExecuteChanged();
        }
    }
}
