using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Core.Services.Protocols;

/// <summary>
/// Publishes protocol events to interested observers or downstream systems.
/// </summary>
public interface IProtocolEventPublisher
{
    /// <summary>
    /// Publishes the specified protocol event.
    /// </summary>
    /// <param name="protocolEvent">The event to publish.</param>
    /// <param name="cancellationToken">A token used to cancel the publish operation.</param>
    Task PublishAsync(IProtocolEvent protocolEvent, CancellationToken cancellationToken = default);
}
