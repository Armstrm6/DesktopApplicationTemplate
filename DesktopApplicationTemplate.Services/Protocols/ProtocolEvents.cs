using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols;

namespace DesktopApplicationTemplate.Services.Protocols;

/// <summary>
/// Represents a lifecycle event emitted by a protocol service.
/// </summary>
public sealed class ProtocolLifecycleEvent : IProtocolEvent
{
    public ProtocolLifecycleEvent(
        string protocolName,
        string descriptorId,
        string displayName,
        ProtocolLifecycleStage stage,
        IReadOnlyCollection<string> associatedServices)
    {
        ProtocolName = string.IsNullOrWhiteSpace(protocolName) ? "UnknownProtocol" : protocolName;
        DescriptorId = descriptorId ?? throw new ArgumentNullException(nameof(descriptorId));
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        Stage = stage;
        AssociatedServices = associatedServices ?? Array.Empty<string>();
        Timestamp = DateTimeOffset.UtcNow;

        var metadata = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Protocol"] = ProtocolName,
            ["DescriptorId"] = DescriptorId,
            ["DisplayName"] = DisplayName,
            ["Stage"] = Stage.ToString(),
            ["AssociatedServices"] = AssociatedServices
        };

        Metadata = new ReadOnlyDictionary<string, object?>(metadata);
    }

    public string ProtocolName { get; }

    public string DescriptorId { get; }

    public string DisplayName { get; }

    public ProtocolLifecycleStage Stage { get; }

    public IReadOnlyCollection<string> AssociatedServices { get; }

    public string Name => Stage switch
    {
        ProtocolLifecycleStage.Starting => "ProtocolStarting",
        ProtocolLifecycleStage.Started => "ProtocolStarted",
        ProtocolLifecycleStage.Stopping => "ProtocolStopping",
        ProtocolLifecycleStage.Stopped => "ProtocolStopped",
        _ => "ProtocolLifecycle"
    };

    public DateTimeOffset Timestamp { get; }

    public IReadOnlyDictionary<string, object?> Metadata { get; }
}

/// <summary>
/// Represents a log entry emitted by a protocol service.
/// </summary>
public sealed class ProtocolLogEvent : IProtocolEvent
{
    public ProtocolLogEvent(string protocolName, LogLevel level, string message, Exception? exception = null)
    {
        ProtocolName = string.IsNullOrWhiteSpace(protocolName) ? "UnknownProtocol" : protocolName;
        Level = level;
        Message = string.IsNullOrWhiteSpace(message) ? "No message provided." : message;
        Exception = exception;
        Timestamp = DateTimeOffset.UtcNow;

        var metadata = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Protocol"] = ProtocolName,
            ["Level"] = Level,
            ["Message"] = Message
        };

        if (Exception is not null)
        {
            metadata["Exception"] = Exception.ToString();
        }

        Metadata = new ReadOnlyDictionary<string, object?>(metadata);
    }

    public string ProtocolName { get; }

    public LogLevel Level { get; }

    public string Message { get; }

    public Exception? Exception { get; }

    public string Name => "ProtocolLog";

    public DateTimeOffset Timestamp { get; }

    public IReadOnlyDictionary<string, object?> Metadata { get; }
}

/// <summary>
/// Identifies the lifecycle stage represented by a <see cref="ProtocolLifecycleEvent"/>.
/// </summary>
public enum ProtocolLifecycleStage
{
    Starting,
    Started,
    Stopping,
    Stopped
}
