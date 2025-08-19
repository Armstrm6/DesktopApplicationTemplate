using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DesktopApplicationTemplate.UI.Models;

/// <summary>
/// Represents a tag subscription with an associated outgoing test message.
/// </summary>
public class TagSubscription : INotifyPropertyChanged
{
    private string _tag = string.Empty;

    /// <summary>
    /// Tag or topic to subscribe to.
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

    private string _outgoingMessage = string.Empty;

    /// <summary>
    /// Message published when testing this tag.
    /// </summary>
    public string OutgoingMessage
    {
        get => _outgoingMessage;
        set
        {
            _outgoingMessage = value;
            OnPropertyChanged();
        }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
