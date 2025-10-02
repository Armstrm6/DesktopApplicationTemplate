using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Core.Services.Protocols;

/// <summary>
/// Provides lifecycle hooks that can observe the state changes of a protocol service.
/// </summary>
public interface IProtocolLifecycleObserver
{
    /// <summary>
    /// Invoked before a protocol service begins its startup sequence.
    /// </summary>
    /// <param name="protocolService">The protocol service undergoing startup.</param>
    /// <param name="cancellationToken">A token used to cancel the hook execution.</param>
    Task OnStartingAsync(IProtocolService protocolService, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invoked after a protocol service has completed its startup sequence.
    /// </summary>
    /// <param name="protocolService">The protocol service that has started.</param>
    /// <param name="cancellationToken">A token used to cancel the hook execution.</param>
    Task OnStartedAsync(IProtocolService protocolService, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invoked before a protocol service begins its shutdown sequence.
    /// </summary>
    /// <param name="protocolService">The protocol service undergoing shutdown.</param>
    /// <param name="cancellationToken">A token used to cancel the hook execution.</param>
    Task OnStoppingAsync(IProtocolService protocolService, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invoked after a protocol service has completed its shutdown sequence.
    /// </summary>
    /// <param name="protocolService">The protocol service that has stopped.</param>
    /// <param name="cancellationToken">A token used to cancel the hook execution.</param>
    Task OnStoppedAsync(IProtocolService protocolService, CancellationToken cancellationToken = default);
}
