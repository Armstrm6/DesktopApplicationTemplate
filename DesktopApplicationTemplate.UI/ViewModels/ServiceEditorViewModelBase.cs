using System;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Helpers;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// Provides shared command wiring for service editor view models used for both create and edit flows.
/// </summary>
/// <typeparam name="TOptions">Type of options managed by the service.</typeparam>
public abstract class ServiceEditorViewModelBase<TOptions> : ValidatableViewModelBase, ILoggingViewModel
{
    private string _serviceName = string.Empty;
    private ServiceType _serviceType;

    protected ServiceEditorViewModelBase(IServiceRule rule, ILoggingService? logger = null)
    {
        Rule = rule ?? throw new ArgumentNullException(nameof(rule));
        Logger = logger;
        var saveCommand = new AsyncRelayCommand(OnSaveAsync, () => !HasErrors);
        SaveCommand = saveCommand;
        CancelCommand = new RelayCommand(OnCancel);
        AdvancedConfigCommand = new RelayCommand(OnAdvancedConfig);
        SaveButtonText = "Save";
        ErrorsChanged += (_, _) => saveCommand.RaiseCanExecuteChanged();
    }

    /// <inheritdoc />
    public ILoggingService? Logger { get; set; }

    /// <summary>
    /// Provides validation helpers for derived classes.
    /// </summary>
    protected IServiceRule Rule { get; }

    /// <summary>
    /// Name of the service being edited or created.
    /// </summary>
    public string ServiceName
    {
        get => _serviceName;
        set
        {
            _serviceName = value;
            var error = Rule.ValidateRequired(value, "Service name");
            if (error is not null)
                AddError(nameof(ServiceName), error);
            else
                ClearErrors(nameof(ServiceName));
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Type of the service being edited or created.
    /// </summary>
    public ServiceType ServiceType
    {
        get => _serviceType;
        set { _serviceType = value; OnPropertyChanged(); }
    }

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
    /// Raised when the service is saved.
    /// </summary>
    public event Func<string, TOptions, Task>? ServiceSaved;

    /// <summary>
    /// Raised when editing is cancelled.
    /// </summary>
    public event Action? EditCancelled;

    /// <summary>
    /// Raised when advanced configuration is requested.
    /// </summary>
    public event Action<TOptions>? AdvancedConfigRequested;

    /// <summary>
    /// Raises the <see cref="ServiceSaved"/> event with the provided options.
    /// </summary>
    /// <param name="options">Options to include with the event.</param>
    protected Task RaiseServiceSavedAsync(TOptions options) =>
        InvokeServiceSavedAsync(ServiceSaved, ServiceName, options);

    /// <summary>
    /// Raises the <see cref="EditCancelled"/> event.
    /// </summary>
    protected void RaiseEditCancelled() =>
        EditCancelled?.Invoke();

    /// <summary>
    /// Raises the <see cref="AdvancedConfigRequested"/> event with the provided options.
    /// </summary>
    /// <param name="options">Options to include with the event.</param>
    protected void RaiseAdvancedConfigRequested(TOptions options) =>
        AdvancedConfigRequested?.Invoke(options);

    /// <summary>
    /// Handles save operations for the service.
    /// </summary>
    protected abstract Task OnSaveAsync();

    /// <summary>
    /// Handles cancellation of the operation.
    /// </summary>
    protected abstract void OnCancel();

    /// <summary>
    /// Handles requests for advanced configuration.
    /// </summary>
    protected abstract void OnAdvancedConfig();
    private static Task InvokeServiceSavedAsync(Func<string, TOptions, Task>? handler, string serviceName, TOptions options)
    {
        if (handler is null)
        {
            return Task.CompletedTask;
        }

        var invocationList = handler.GetInvocationList();
        if (invocationList.Length == 1)
        {
            var task = ((Func<string, TOptions, Task>)invocationList[0])(serviceName, options);
            return task ?? Task.CompletedTask;
        }

        var tasks = new Task[invocationList.Length];
        for (var i = 0; i < invocationList.Length; i++)
        {
            tasks[i] = ((Func<string, TOptions, Task>)invocationList[i])(serviceName, options) ?? Task.CompletedTask;
        }

        return Task.WhenAll(tasks);
    }
}
