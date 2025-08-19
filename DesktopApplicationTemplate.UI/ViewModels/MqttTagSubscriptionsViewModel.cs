using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// View model for managing MQTT topic subscriptions and test messages.
/// </summary>
public class MqttTagSubscriptionsViewModel : ValidatableViewModelBase, ILoggingViewModel
{
    private readonly MqttService _service;

    private string _newTopic = string.Empty;
    private TagSubscription? _selectedSubscription;
    private bool _isConnected;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttTagSubscriptionsViewModel"/> class.
    /// </summary>
    public MqttTagSubscriptionsViewModel(MqttService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));

        TagSubscriptions = new ObservableCollection<TagSubscription>();
        AddTopicCommand = new RelayCommand(AddTopic);
        RemoveTopicCommand = new RelayCommand(RemoveTopic, () => SelectedSubscription != null);
        ConnectCommand = new AsyncRelayCommand(ConnectAsync);
        PublishTestMessageCommand = new AsyncRelayCommand(PublishTestAsync, CanPublishTest);
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Subscriptions maintained by this service.
    /// </summary>
    public ObservableCollection<TagSubscription> TagSubscriptions { get; }

    /// <summary>
    /// Gets or sets the new topic entry.
    /// </summary>
    public string NewTopic
    {
        get => _newTopic;
        set { _newTopic = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Gets or sets the selected subscription.
    /// </summary>
    public TagSubscription? SelectedSubscription
    {
        get => _selectedSubscription;
        set
        {
            if (_selectedSubscription != null)
            {
                _selectedSubscription.PropertyChanged -= SelectedSubscription_PropertyChanged;
            }
            _selectedSubscription = value;
            if (_selectedSubscription != null)
            {
                _selectedSubscription.PropertyChanged += SelectedSubscription_PropertyChanged;
            }
            OnPropertyChanged();
            ((RelayCommand)RemoveTopicCommand).RaiseCanExecuteChanged();
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
        TagSubscriptions.Add(new TagSubscription { Tag = NewTopic });
        NewTopic = string.Empty;
    }

    private void RemoveTopic()
    {
        if (SelectedSubscription is null)
            return;
        TagSubscriptions.Remove(SelectedSubscription);
        SelectedSubscription = null;
    }

    private bool CanPublishTest() => SelectedSubscription != null && !string.IsNullOrWhiteSpace(SelectedSubscription.OutgoingMessage);

    /// <summary>
    /// Connects to the MQTT broker.
    /// </summary>
    public async Task ConnectAsync()
    {
        Logger?.Log("MQTT connect start", LogLevel.Debug);
        await _service.ConnectAsync().ConfigureAwait(false);
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
        await _service.PublishAsync(SelectedSubscription!.Tag, SelectedSubscription.OutgoingMessage).ConfigureAwait(false);
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
