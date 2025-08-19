using System;
using System.Collections.ObjectModel;
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
/// View model for managing MQTT tag subscriptions and test publishing.
/// </summary>
public class MqttTagSubscriptionsViewModel : ValidatableViewModelBase, ILoggingViewModel
{
    private readonly MqttService _service;
    private TagSubscription? _selectedSubscription;
    private string _newTag = string.Empty;
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

        AddTopicCommand = new RelayCommand(AddTopic, CanAddTopic);
        RemoveTopicCommand = new RelayCommand(RemoveTopic, () => SelectedSubscription != null);
        ConnectCommand = new AsyncRelayCommand(ConnectAsync);
        PublishTestMessageCommand = new AsyncRelayCommand(PublishTestAsync, CanPublishTest);
        TestTagEndpointCommand = new AsyncRelayCommand<TagSubscription?>(TestTagEndpointAsync, CanTestTagEndpoint);
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>Current tag subscriptions.</summary>
    public ObservableCollection<TagSubscription> Subscriptions { get; }

    /// <summary>Results of subscription attempts for UI feedback.</summary>
    public ObservableCollection<SubscriptionResult> SubscriptionResults { get; }

    /// <summary>New tag identifier.</summary>
    public string NewTag
    {
        get => _newTag;
        set { _newTag = value; OnPropertyChanged(); ((RelayCommand)AddTopicCommand).RaiseCanExecuteChanged(); }
    }

    /// <summary>New topic to subscribe to.</summary>
    public string NewTopic
    {
        get => _newTopic;
        set { _newTopic = value; OnPropertyChanged(); ((RelayCommand)AddTopicCommand).RaiseCanExecuteChanged(); }
    }

    /// <summary>QoS level for new subscriptions.</summary>
    public MqttQualityOfServiceLevel NewQoS
    {
        get => _newQoS;
        set { _newQoS = value; OnPropertyChanged(); }
    }

    /// <summary>Currently selected subscription.</summary>
    public TagSubscription? SelectedSubscription
    {
        get => _selectedSubscription;
        set
        {
            if (_selectedSubscription == value) return;
            if (_selectedSubscription != null)
                _selectedSubscription.PropertyChanged -= SelectedSubscriptionOnPropertyChanged;
            _selectedSubscription = value;
            if (_selectedSubscription != null)
                _selectedSubscription.PropertyChanged += SelectedSubscriptionOnPropertyChanged;
            OnPropertyChanged();
            ((RelayCommand)RemoveTopicCommand).RaiseCanExecuteChanged();
            ((AsyncRelayCommand)PublishTestMessageCommand).RaiseCanExecuteChanged();
        }
    }

    /// <summary>Command to add a new topic subscription.</summary>
    public ICommand AddTopicCommand { get; }

    /// <summary>Command to remove the selected subscription.</summary>
    public ICommand RemoveTopicCommand { get; }

    /// <summary>Command to connect to the broker and subscribe to topics.</summary>
    public AsyncRelayCommand ConnectCommand { get; }

    /// <summary>Command to publish the selected tag's test message.</summary>
    public AsyncRelayCommand PublishTestMessageCommand { get; }

    /// <summary>Command to test a tag's endpoint directly from the list.</summary>
    public AsyncRelayCommand<TagSubscription?> TestTagEndpointCommand { get; }

    private bool CanAddTopic() => !string.IsNullOrWhiteSpace(NewTag) && !string.IsNullOrWhiteSpace(NewTopic);

    private void AddTopic()
    {
        var sub = new TagSubscription
        {
            Tag = NewTag,
            Topic = NewTopic,
            QoS = NewQoS
        };
        Subscriptions.Add(sub);
        _service.UpdateTagSubscription(sub);
        NewTag = string.Empty;
        NewTopic = string.Empty;
    }

    private void RemoveTopic()
    {
        if (SelectedSubscription is null) return;
        Subscriptions.Remove(SelectedSubscription);
    }

    public async Task ConnectAsync()
    {
        await _service.ConnectAsync().ConfigureAwait(false);
        foreach (var sub in Subscriptions)
        {
            await _service.SubscribeAsync(sub.Topic, sub.QoS).ConfigureAwait(false);
        }
    }

    private bool CanPublishTest() => SelectedSubscription is { Endpoint.Length: > 0, OutgoingMessage.Length: > 0 };

    private Task PublishTestAsync()
    {
        return SelectedSubscription is null
            ? Task.CompletedTask
            : _service.PublishAsync(SelectedSubscription.Endpoint, SelectedSubscription.OutgoingMessage);
    }

    private bool CanTestTagEndpoint(TagSubscription? sub) => sub is { Endpoint.Length: > 0, OutgoingMessage.Length: > 0 };

    private Task TestTagEndpointAsync(TagSubscription? sub)
        => sub is null ? Task.CompletedTask : _service.PublishAsync(sub.Endpoint, sub.OutgoingMessage);

    private void SelectedSubscriptionOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TagSubscription.Endpoint) || e.PropertyName == nameof(TagSubscription.OutgoingMessage))
        {
            ((AsyncRelayCommand)PublishTestMessageCommand).RaiseCanExecuteChanged();
        }
    }
}

