using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.ViewModels;

/// <summary>
/// Provides shared command wiring for service edit view models.
/// </summary>
/// <typeparam name="TOptions">Type of options managed by the service.</typeparam>
public abstract class ServiceEditViewModelBase<TOptions> : ServiceEditorViewModelBase<TOptions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceEditViewModelBase{TOptions}"/> class.
    /// </summary>
    protected ServiceEditViewModelBase(IServiceRule rule, ILoggingService? logger = null)
        : base(rule, logger)
    {
    }
}
