namespace DesktopApplicationTemplate.Core.Services;

/// <summary>
/// Creates <see cref="IServiceRuntime"/> instances for descriptor driven services.
/// </summary>
public interface IServiceRuntimeFactory
{
    /// <summary>
    /// Creates a runtime instance for the supplied context.
    /// </summary>
    /// <param name="context">Activation metadata describing the requested service.</param>
    /// <returns>A configured runtime instance.</returns>
    IServiceRuntime Create(ServiceRuntimeContext context);
}
