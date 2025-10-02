using System;

namespace DesktopApplicationTemplate.Core.Services.Protocols;

/// <summary>
/// Provides a protocol-specific logging abstraction that is independent of external frameworks.
/// </summary>
public interface IProtocolLogger
{
    /// <summary>
    /// Records a protocol informational message.
    /// </summary>
    /// <param name="protocolService">The source protocol service.</param>
    /// <param name="message">The message to record.</param>
    void LogInformation(IProtocolService protocolService, string message);

    /// <summary>
    /// Records a protocol warning message.
    /// </summary>
    /// <param name="protocolService">The source protocol service.</param>
    /// <param name="message">The message to record.</param>
    void LogWarning(IProtocolService protocolService, string message);

    /// <summary>
    /// Records a protocol error message with the associated exception.
    /// </summary>
    /// <param name="protocolService">The source protocol service.</param>
    /// <param name="exception">The exception to include with the log entry.</param>
    /// <param name="message">The message to record.</param>
    void LogError(IProtocolService protocolService, Exception exception, string message);
}
