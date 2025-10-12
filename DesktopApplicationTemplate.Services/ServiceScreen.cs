using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.Services;

/// <summary>
/// Default implementation of <see cref="IServiceScreen{TOptions}"/>.
/// </summary>
/// <typeparam name="TOptions">Type of options managed by the screen.</typeparam>
[SupportedOSPlatform("windows")]
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
    public event Func<string, TOptions, Task>? ServiceSaved;

    /// <inheritdoc />
    public event Action? EditCancelled;

    /// <inheritdoc />
    public event Action<TOptions>? AdvancedConfigRequested;

    /// <inheritdoc />
    public async Task SaveAsync(string serviceName, TOptions options)
    {
        _logger?.Log($"Saving service {serviceName}", LogLevel.Debug);
        await ServiceScreenEventInvoker.InvokeServiceSavedAsync(ServiceSaved, serviceName, options).ConfigureAwait(false);
        _logger?.Log($"Saved service {serviceName}", LogLevel.Debug);
    }

    /// <inheritdoc />
    public void Cancel()
    {
        _logger?.Log("Service creation cancelled", LogLevel.Debug);
        EditCancelled?.Invoke();
    }

    /// <inheritdoc />
    public void OpenAdvanced(TOptions options)
    {
        _logger?.Log("Opening advanced configuration", LogLevel.Debug);
        AdvancedConfigRequested?.Invoke(options);
    }
}

internal static class ServiceScreenEventInvoker
{
    internal static Task InvokeServiceSavedAsync<TOptions>(Func<string, TOptions, Task>? handler, string serviceName, TOptions options)
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

