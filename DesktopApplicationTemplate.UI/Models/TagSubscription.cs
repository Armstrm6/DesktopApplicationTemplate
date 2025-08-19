
using System.ComponentModel;
using MQTTnet.Protocol;

namespace DesktopApplicationTemplate.UI.Models;

/// <summary>
/// Represents a topic subscription with QoS and an outgoing test message.
/// </summary>
public class TagSubscription : INotifyPropertyChanged
{
    private string _topic = string.Empty;
    private MqttQualityOfServiceLevel _qos;
    private string _outgoingMessage = string.Empty;

    /// <summary>
    /// Gets or sets the topic.
    /// </summary>
    public string Topic
    {
        get => _topic;
        set
        {
            if (_topic == value) return;
            _topic = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Topic)));
        }
    }

    /// <summary>
    /// Gets or sets the quality of service level.
    /// </summary>
    public MqttQualityOfServiceLevel QoS
    {
        get => _qos;
        set
        {
            if (_qos == value) return;
            _qos = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(QoS)));
        }
    }

    /// <summary>
    /// Gets or sets the outgoing message used for test publishing.
    /// </summary>
    public string OutgoingMessage
    {
        get => _outgoingMessage;
        set
        {
            if (_outgoingMessage == value) return;
            _outgoingMessage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OutgoingMessage)));
        }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
}
