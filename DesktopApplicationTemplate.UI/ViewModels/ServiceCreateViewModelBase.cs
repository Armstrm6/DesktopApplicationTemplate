using System.Windows.Input;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// Provides shared command wiring for service creation view models.
/// </summary>
/// <typeparam name="TOptions">Type of options managed by the service.</typeparam>
public abstract class ServiceCreateViewModelBase<TOptions> : ServiceEditorViewModelBase<TOptions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceCreateViewModelBase{TOptions}"/> class.
    /// </summary>
    protected ServiceCreateViewModelBase(IServiceRule rule, ILoggingService? logger = null)
        : base(rule, logger)
    {
        SaveButtonText = "Create";
    }

    /// <summary>
    /// Alias for <see cref="ServiceEditorViewModelBase{TOptions}.SaveCommand"/> used by XAML bindings.
    /// </summary>
    public ICommand CreateCommand => SaveCommand;
}
