using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// Base class that implements <see cref="INotifyPropertyChanged"/> to reduce
/// repetitive code across view models.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Notifies listeners that a property value has changed.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

