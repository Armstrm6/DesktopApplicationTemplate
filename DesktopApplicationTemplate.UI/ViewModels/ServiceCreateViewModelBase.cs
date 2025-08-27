using System.Windows.Input;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// Provides shared command wiring for service creation view models.
/// </summary>
/// <typeparam name="TOptions">Type of options managed by the service.</typeparam>
public abstract class ServiceCreateViewModelBase<TOptions> : ValidatableViewModelBase, ILoggingViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceCreateViewModelBase{TOptions}"/> class.
    /// </summary>
    /// <param name="logger">Optional logging service.</param>
    protected ServiceCreateViewModelBase(ILoggingService? logger = null)
    {
        Logger = logger;
        SaveCommand = new RelayCommand(OnSave);
        CancelCommand = new RelayCommand(OnCancel);
        AdvancedConfigCommand = new RelayCommand(OnAdvancedConfig);
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Command executed to save the service.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Command executed to cancel creation.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Command executed to open advanced configuration.
    /// </summary>
    public ICommand AdvancedConfigCommand { get; }

    /// <summary>
    /// Handles save operations for the service.
    /// </summary>
    protected abstract void OnSave();

    /// <summary>
    /// Handles cancellation of the operation.
    /// </summary>
    protected abstract void OnCancel();

    /// <summary>
    /// Handles requests for advanced configuration.
    /// </summary>
    protected abstract void OnAdvancedConfig();
}

