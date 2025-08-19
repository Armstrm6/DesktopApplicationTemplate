using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MQTTnet.Protocol;

namespace DesktopApplicationTemplate.UI.Models;

/// <summary>
/// Represents an MQTT topic subscription with test publishing metadata.
/// </summary>
public class TagSubscription : INotifyPropertyChanged
{
    private string _topic = string.Empty;
    private MqttQualityOfServiceLevel _qoS;
    private string _endpoint = string.Empty;
    private string _outgoingMessage = string.Empty;
    private string? _statusColor;
    private string? _icon;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagSubscription"/> class.
    /// </summary>
    public TagSubscription()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TagSubscription"/> class with the specified topic.
    /// </summary>
    public TagSubscription(string topic)
    {
        _topic = topic ?? throw new ArgumentNullException(nameof(topic));
    }

    /// <summary>
    /// Gets or sets the MQTT topic.
    /// </summary>
    public string Topic
    {
        get => _topic;
        set
        {
            if (_topic == value) return;
            _topic = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the quality of service level.
    /// </summary>
    public MqttQualityOfServiceLevel QoS
    {
        get => _qoS;
        set
        {
            if (_qoS == value) return;
            _qoS = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the endpoint for test publishing.
    /// </summary>
    public string Endpoint
    {
        get => _endpoint;
        set
        {
            if (_endpoint == value) return;
            _endpoint = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the outgoing test message.
    /// </summary>
    public string OutgoingMessage
    {
        get => _outgoingMessage;
        set
        {
            if (_outgoingMessage == value) return;
            _outgoingMessage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the status color for UI display.
    /// </summary>
    public string? StatusColor
    {
        get => _statusColor;
        set
        {
            if (_statusColor == value) return;
            _statusColor = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the icon for UI display.
    /// </summary>
    public string? Icon
    {
        get => _icon;
        set
        {
            if (_icon == value) return;
            _icon = value;
            OnPropertyChanged();
        }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
