using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Core.Services.Protocols;

/// <summary>
/// Defines the base contract for services that communicate using a specific protocol.
/// </summary>
public interface IProtocolService
{
    /// <summary>
    /// Gets the unique name of the protocol implementation.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a value indicating whether the protocol service is currently active.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Starts the protocol service and performs any required initialization.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the startup operation.</param>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the protocol service and releases allocated resources.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the shutdown operation.</param>
    Task StopAsync(CancellationToken cancellationToken = default);
}
