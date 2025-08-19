using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DesktopApplicationTemplate.UI.Models;

/// <summary>
/// Represents a subscribed MQTT tag with optional styling metadata.
/// </summary>
public class TagSubscription : INotifyPropertyChanged
{
    private string? _statusColor;
    private string? _icon;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagSubscription"/> class.
    /// </summary>
    /// <param name="topic">The MQTT topic.</param>
    public TagSubscription(string topic)
    {
        Topic = topic ?? throw new ArgumentNullException(nameof(topic));
    }

    /// <summary>
    /// Gets the MQTT topic for this subscription.
    /// </summary>
    public string Topic { get; }

    /// <summary>
    /// Gets or sets the status color associated with the tag.
    /// </summary>
    public string? StatusColor
    {
        get => _statusColor;
        set
        {
            _statusColor = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets an optional icon representing the tag.
    /// </summary>
    public string? Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged();
        }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
