using System.Windows.Input;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// Provides shared command wiring for service editor view models used for both create and edit flows.
/// </summary>
/// <typeparam name="TOptions">Type of options managed by the service.</typeparam>
public abstract class ServiceEditorViewModelBase<TOptions> : ValidatableViewModelBase, ILoggingViewModel
{
    protected ServiceEditorViewModelBase(ILoggingService? logger = null)
    {
        Logger = logger;
        var saveCommand = new RelayCommand(OnSave, () => !HasErrors);
        SaveCommand = saveCommand;
        CancelCommand = new RelayCommand(OnCancel);
        AdvancedConfigCommand = new RelayCommand(OnAdvancedConfig);
        SaveButtonText = "Save";
        ErrorsChanged += (_, _) => saveCommand.RaiseCanExecuteChanged();
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Command executed to save or create the service.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Command executed to cancel editing or creation.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Command executed to open advanced configuration.
    /// </summary>
    public ICommand AdvancedConfigCommand { get; }

    /// <summary>
    /// Text displayed on the save button.
    /// </summary>
    public string SaveButtonText { get; protected set; }

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
