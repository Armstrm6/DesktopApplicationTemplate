using System;
using System.Windows.Input;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// Base view model providing Save and Back commands for advanced configuration screens.
/// </summary>
/// <typeparam name="TOptions">Type of options managed by the screen.</typeparam>
public abstract class AdvancedConfigViewModelBase<TOptions> : ValidatableViewModelBase, ILoggingViewModel
    where TOptions : class
{
    protected AdvancedConfigViewModelBase(ILoggingService? logger = null)
    {
        Logger = logger;
        SaveCommand = new RelayCommand(Save);
        BackCommand = new RelayCommand(Back);
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Command executed to save the configuration.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Command executed to navigate back without saving.
    /// </summary>
    public ICommand BackCommand { get; }

    /// <summary>
    /// Raised when the configuration is saved.
    /// </summary>
    public event Action<TOptions>? Saved;

    /// <summary>
    /// Raised when navigation back is requested.
    /// </summary>
    public event Action? BackRequested;

    private void Save()
    {
        var options = OnSave();
        if (options is not null)
            Saved?.Invoke(options);
    }

    private void Back()
    {
        OnBack();
        BackRequested?.Invoke();
    }

    /// <summary>
    /// Handles saving the configuration and returns updated options or <c>null</c> to cancel.
    /// </summary>
    protected abstract TOptions? OnSave();

    /// <summary>
    /// Handles back navigation.
    /// </summary>
    protected virtual void OnBack()
    {
    }
}
