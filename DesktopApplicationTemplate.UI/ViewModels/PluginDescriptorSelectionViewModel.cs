using System;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// Represents a selectable descriptor in the export dialog.
/// </summary>
public sealed class PluginDescriptorSelectionViewModel : ViewModelBase
{
    private bool _isSelected;

    public PluginDescriptorSelectionViewModel(string id, string displayName, string category)
    {
        Id = id;
        DisplayName = displayName;
        Category = category;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public string Category { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public event EventHandler? SelectionChanged;
}
