
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DesktopApplicationTemplate.UI.Models;

/// <summary>
/// Represents a subscription to a tag with test publish details.
/// </summary>
public class TagSubscription : INotifyPropertyChanged
{
    private string _tag = string.Empty;
    /// <summary>
    /// Identifier of the tag.
    /// </summary>
    public string Tag
    {
        get => _tag;
        set
        {
            _tag = value;
            OnPropertyChanged();
        }
    }

    private string _endpoint = string.Empty;
    /// <summary>
    /// MQTT endpoint used for testing this tag.
    /// </summary>
    public string Endpoint
    {
        get => _endpoint;
        set
        {
            _endpoint = value;
            OnPropertyChanged();
        }
    }

    private string _outgoingMessage = string.Empty;
    /// <summary>
    /// Test message sent when validating the tag's endpoint.
    /// </summary>
    public string OutgoingMessage
    {
        get => _outgoingMessage;
        set
        {
            _outgoingMessage = value;
            OnPropertyChanged();

            if (_outgoingMessage == value) return;
            _outgoingMessage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OutgoingMessage)));
        }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

}
