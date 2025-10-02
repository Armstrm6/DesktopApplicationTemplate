using System;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Heartbeat;

/// <summary>
/// Provides timer-driven heartbeat message generation independent of the UI layer.
/// </summary>
public interface IHeartbeatService : IProtocolService
{
    /// <summary>
    /// Raised whenever a heartbeat payload is generated.
    /// </summary>
    event EventHandler<string>? HeartbeatGenerated;

    /// <summary>
    /// Configures the heartbeat runtime.
    /// </summary>
    /// <param name="serviceName">The service name associated with the runtime.</param>
    /// <param name="options">The heartbeat options.</param>
    /// <param name="interval">The interval between heartbeat emissions.</param>
    /// <param name="cancellationToken">A token used to cancel the configuration.</param>
    Task ConfigureAsync(string serviceName, HeartbeatServiceOptions options, TimeSpan interval, CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a single heartbeat payload without starting the timer.
    /// </summary>
    /// <param name="options">The options describing the heartbeat.</param>
    /// <returns>The formatted heartbeat message.</returns>
    string BuildHeartbeatMessage(HeartbeatServiceOptions options);
}
