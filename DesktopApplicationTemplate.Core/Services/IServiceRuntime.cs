using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Core.Services;

/// <summary>
/// Represents a runtime workflow that can be started and stopped by the host.
/// </summary>
public interface IServiceRuntime : IAsyncDisposable
{
    /// <summary>
    /// Starts the runtime workflow.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel startup.</param>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stops the runtime workflow.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel shutdown.</param>
    Task StopAsync(CancellationToken cancellationToken);
}
