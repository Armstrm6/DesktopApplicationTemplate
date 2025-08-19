using System;
using System.Collections.ObjectModel;
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

    private string _newTopic = string.Empty;
    private TagSubscription? _selectedTopic;
    private string _testMessage = string.Empty;
    private bool _isConnected;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttTagSubscriptionsViewModel"/> class.
    /// </summary>
    public MqttTagSubscriptionsViewModel(MqttService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));

        Topics = new ObservableCollection<TagSubscription>();
        AddTopicCommand = new RelayCommand(AddTopic);
        RemoveTopicCommand = new RelayCommand(RemoveTopic, () => SelectedTopic != null);
        ConnectCommand = new AsyncRelayCommand(ConnectAsync);
        PublishTestMessageCommand = new AsyncRelayCommand(PublishTestAsync, CanPublishTest);
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Topics subscribed to by this service.
    /// </summary>
    public ObservableCollection<TagSubscription> Topics { get; }

    /// <summary>
    /// Gets or sets the new topic entry.
    /// </summary>
    public string NewTopic
    {
        get => _newTopic;
        set { _newTopic = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Gets or sets the selected topic.
    /// </summary>
    public TagSubscription? SelectedTopic
    {
        get => _selectedTopic;
        set
        {
            _selectedTopic = value;
            OnPropertyChanged();
            ((RelayCommand)RemoveTopicCommand).RaiseCanExecuteChanged();
            ((AsyncRelayCommand)PublishTestMessageCommand).RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Gets or sets the test message sent to the selected topic.
    /// </summary>
    public string TestMessage
    {
        get => _testMessage;
        set
        {
            _testMessage = value;
            OnPropertyChanged();
            ((AsyncRelayCommand)PublishTestMessageCommand).RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Gets a value indicating whether the service is connected.
    /// </summary>
    public bool IsConnected
    {
        get => _isConnected;
        private set { _isConnected = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Command to add a topic subscription.
    /// </summary>
    public ICommand AddTopicCommand { get; }

    /// <summary>
    /// Command to remove the selected topic subscription.
    /// </summary>
    public ICommand RemoveTopicCommand { get; }

    /// <summary>
    /// Command to connect to the broker.
    /// </summary>
    public ICommand ConnectCommand { get; }

    /// <summary>
    /// Command to publish a test message to the selected topic.
    /// </summary>
    public ICommand PublishTestMessageCommand { get; }

    private void AddTopic()
    {
        if (string.IsNullOrWhiteSpace(NewTopic))
            return;
        Topics.Add(new TagSubscription { Topic = NewTopic, QoS = MqttQualityOfServiceLevel.AtMostOnce });
        NewTopic = string.Empty;
    }

    private void RemoveTopic()
    {
        if (SelectedTopic is null)
            return;
        Topics.Remove(SelectedTopic);
        SelectedTopic = null;
    }

    private bool CanPublishTest() => SelectedTopic != null && !string.IsNullOrWhiteSpace(TestMessage);

    /// <summary>
    /// Connects to the MQTT broker.
    /// </summary>
    public async Task ConnectAsync()
    {
        Logger?.Log("MQTT connect start", LogLevel.Debug);
        await _service.ConnectAsync().ConfigureAwait(false);
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
        await _service.PublishAsync(SelectedTopic!.Topic, TestMessage).ConfigureAwait(false);
        Logger?.Log("MQTT test publish finished", LogLevel.Debug);
    }
}
