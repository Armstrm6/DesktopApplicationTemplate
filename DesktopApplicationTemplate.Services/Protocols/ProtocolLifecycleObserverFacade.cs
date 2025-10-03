using System;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols;

namespace DesktopApplicationTemplate.Services.Protocols;

/// <summary>
/// Logs protocol lifecycle transitions through the shared logging service.
/// </summary>
public sealed class ProtocolLifecycleObserverFacade : IProtocolLifecycleObserver
{
    private readonly ILoggingService _loggingService;

    public ProtocolLifecycleObserverFacade(ILoggingService loggingService)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
    }

    /// <inheritdoc />
    public Task OnStartingAsync(IProtocolService protocolService, CancellationToken cancellationToken = default)
        => LogAsync(protocolService, "starting");

    /// <inheritdoc />
    public Task OnStartedAsync(IProtocolService protocolService, CancellationToken cancellationToken = default)
        => LogAsync(protocolService, "started");

    /// <inheritdoc />
    public Task OnStoppingAsync(IProtocolService protocolService, CancellationToken cancellationToken = default)
        => LogAsync(protocolService, "stopping");

    /// <inheritdoc />
    public Task OnStoppedAsync(IProtocolService protocolService, CancellationToken cancellationToken = default)
        => LogAsync(protocolService, "stopped");

    private Task LogAsync(IProtocolService protocolService, string stage)
    {
        if (protocolService is null)
        {
            throw new ArgumentNullException(nameof(protocolService));
        }

        var protocolName = string.IsNullOrWhiteSpace(protocolService.Name)
            ? "UnknownProtocol"
            : protocolService.Name;

        _loggingService.Log($"[{protocolName}] Lifecycle {stage}", LogLevel.Information);
        return Task.CompletedTask;
    }
}
