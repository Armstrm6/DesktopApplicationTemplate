using System.Windows.Input;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// Provides shared command wiring for service edit view models.
/// </summary>
/// <typeparam name="TOptions">Type of options managed by the service.</typeparam>
public abstract class ServiceEditViewModelBase<TOptions> : ValidatableViewModelBase, ILoggingViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceEditViewModelBase{TOptions}"/> class.
    /// </summary>
    /// <param name="logger">Optional logging service.</param>
    protected ServiceEditViewModelBase(ILoggingService? logger = null)
    {
        Logger = logger;
        SaveCommand = new RelayCommand(OnSave);
        CancelCommand = new RelayCommand(OnCancel);
        AdvancedConfigCommand = new RelayCommand(OnAdvancedConfig);
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Command executed to save changes.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Command executed to cancel editing.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Command executed to open advanced configuration.
    /// </summary>
    public ICommand AdvancedConfigCommand { get; }

    /// <summary>
    /// Saves modifications to the service.
    /// </summary>
    protected abstract void OnSave();

    /// <summary>
    /// Cancels the edit operation.
    /// </summary>
    protected abstract void OnCancel();

    /// <summary>
    /// Opens advanced configuration.
    /// </summary>
    protected abstract void OnAdvancedConfig();
}

