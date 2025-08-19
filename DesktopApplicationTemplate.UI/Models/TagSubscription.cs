using System.ComponentModel;
using System.Runtime.CompilerServices;
using MQTTnet.Protocol;

namespace DesktopApplicationTemplate.UI.Models;

/// <summary>
/// Represents a subscription to an MQTT topic with metadata used by the UI and tests.
/// </summary>
public class TagSubscription : INotifyPropertyChanged
{
    private string _tag = string.Empty;
    private string _topic = string.Empty;
    private string _endpoint = string.Empty;
    private string _outgoingMessage = string.Empty;
    private MqttQualityOfServiceLevel _qoS = MqttQualityOfServiceLevel.AtMostOnce;
    private string? _statusColor;
    private string? _icon;

    /// <summary>Identifier displayed for this subscription.</summary>
    public string Tag
    {
        get => _tag;
        set
        {
            if (_tag == value) return;
            _tag = value;
            OnPropertyChanged();
        }
    }

    /// <summary>MQTT topic associated with the tag.</summary>
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

    /// <summary>Endpoint used when testing this tag.</summary>
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

    /// <summary>Test message published when validating the tag.</summary>
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

    /// <summary>The quality of service level for the subscription.</summary>
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

    /// <summary>Optional status color used for styling.</summary>
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

    /// <summary>Optional icon representing the tag.</summary>
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

