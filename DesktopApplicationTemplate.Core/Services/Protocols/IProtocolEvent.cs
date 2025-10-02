using System;
using System.Collections.Generic;

namespace DesktopApplicationTemplate.Core.Services.Protocols;

/// <summary>
/// Represents a structured event emitted by a protocol service.
/// </summary>
public interface IProtocolEvent
{
    /// <summary>
    /// Gets the name of the event.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the timestamp associated with the event occurrence.
    /// </summary>
    DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets contextual metadata that describes the event payload.
    /// </summary>
    IReadOnlyDictionary<string, object?> Metadata { get; }
}
