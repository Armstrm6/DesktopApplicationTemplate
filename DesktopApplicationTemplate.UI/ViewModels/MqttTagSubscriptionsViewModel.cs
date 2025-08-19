using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;

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

    /// <summary>
    /// Gets or sets the new tag entry.
    /// </summary>
    public string NewTag
    {
        get => _newTag;
        set { _newTag = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Gets or sets the selected subscription.
    /// </summary>
    public TagSubscription? SelectedSubscription
    {
        get => _selectedSubscription;
        set
        {
            _selectedSubscription = value;
            OnPropertyChanged();
            ((RelayCommand)RemoveTagCommand).RaiseCanExecuteChanged();
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

    /// <summary>
    /// Connects to the MQTT broker.
    /// </summary>
    public async Task ConnectAsync()
    {
        Logger?.Log("MQTT connect start", LogLevel.Debug);
        await _service.ConnectAsync().ConfigureAwait(false);
        Logger?.Log("MQTT connect finished", LogLevel.Debug);
    }
}

