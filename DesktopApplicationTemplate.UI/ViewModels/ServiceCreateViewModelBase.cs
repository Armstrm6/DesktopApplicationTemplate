using System;
using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// Provides shared command wiring for service creation view models.
/// </summary>
/// <typeparam name="TOptions">Type of options managed by the service.</typeparam>
public abstract class ServiceCreateViewModelBase<TOptions> : ServiceEditorViewModelBase<TOptions>
    where TOptions : new()
{
    private readonly IServiceScreen<TOptions>? _screen;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceCreateViewModelBase{TOptions}"/> class.
    /// </summary>
    protected ServiceCreateViewModelBase(IServiceRule rule, IServiceScreen<TOptions>? screen = null, ILoggingService? logger = null)
        : base(rule, logger)
    {
        _screen = screen;
        SaveButtonText = "Create";
        if (_screen is not null)
        {
            _screen.ServiceSaved += (_, o) => RaiseServiceSaved(o);
            _screen.EditCancelled += () => RaiseEditCancelled();
            _screen.AdvancedConfigRequested += o => RaiseAdvancedConfigRequested(o);
        }
    }

    /// <summary>
    /// Current configuration options.
    /// </summary>
    public TOptions Options { get; } = new();

    /// <summary>
    /// Alias for <see cref="ServiceEditorViewModelBase{TOptions}.SaveCommand"/> used by XAML bindings.
    /// </summary>
    public ICommand CreateCommand => SaveCommand;

    /// <inheritdoc />
    protected override void OnSave()
    {
        if (HasErrors)
            return;
        ApplyOptions(Options);
        if (_screen is not null)
            _screen.Save(ServiceName, Options);
        else
            RaiseServiceSaved(Options);
    }

    /// <inheritdoc />
    protected override void OnCancel()
    {
        if (_screen is not null)
            _screen.Cancel();
        else
            RaiseEditCancelled();
    }

    /// <inheritdoc />
    protected override void OnAdvancedConfig()
    {
        ApplyOptions(Options);
        if (_screen is not null)
            _screen.OpenAdvanced(Options);
        else
            RaiseAdvancedConfigRequested(Options);
    }

    /// <summary>
    /// Copies view model properties into the provided options instance.
    /// </summary>
    /// <param name="options">Options instance to populate.</param>
    protected abstract void ApplyOptions(TOptions options);
}
