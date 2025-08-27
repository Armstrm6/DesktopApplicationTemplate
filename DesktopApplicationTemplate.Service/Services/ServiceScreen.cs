using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.Service.Services;

/// <summary>
/// Default implementation of <see cref="IServiceScreen{TOptions}"/>.
/// </summary>
/// <typeparam name="TOptions">Type of options managed by the screen.</typeparam>
public class ServiceScreen<TOptions> : IServiceScreen<TOptions>
{
    private readonly ILoggingService? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceScreen{TOptions}"/> class.
    /// </summary>
    /// <param name="logger">Optional logging service.</param>
    public ServiceScreen(ILoggingService? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public event Action<string, TOptions>? Saved;

    /// <inheritdoc />
    public event Action? Cancelled;

    /// <inheritdoc />
    public event Action<TOptions>? AdvancedConfigRequested;

    /// <inheritdoc />
    public void Save(string serviceName, TOptions options)
    {
        _logger?.Log($"Saving service {serviceName}", LogLevel.Debug);
        Saved?.Invoke(serviceName, options);
        _logger?.Log($"Saved service {serviceName}", LogLevel.Debug);
    }

    /// <inheritdoc />
    public void Cancel()
    {
        _logger?.Log("Service creation cancelled", LogLevel.Debug);
        Cancelled?.Invoke();
    }

    /// <inheritdoc />
    public void OpenAdvanced(TOptions options)
    {
        _logger?.Log("Opening advanced configuration", LogLevel.Debug);
        AdvancedConfigRequested?.Invoke(options);
    }
}

