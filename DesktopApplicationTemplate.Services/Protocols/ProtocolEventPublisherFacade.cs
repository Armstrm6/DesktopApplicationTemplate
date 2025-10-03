using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols;

namespace DesktopApplicationTemplate.Services.Protocols;

/// <summary>
/// Publishes protocol events to the shared logging service.
/// </summary>
public sealed class ProtocolEventPublisherFacade : IProtocolEventPublisher
{
    private readonly ILoggingService _loggingService;

    public ProtocolEventPublisherFacade(ILoggingService loggingService)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
    }

    /// <inheritdoc />
    public Task PublishAsync(IProtocolEvent protocolEvent, CancellationToken cancellationToken = default)
    {
        if (protocolEvent is null)
        {
            throw new ArgumentNullException(nameof(protocolEvent));
        }

        var message = BuildMessage(protocolEvent);
        _loggingService.Log(message, LogLevel.Information);
        return Task.CompletedTask;
    }

    private static string BuildMessage(IProtocolEvent protocolEvent)
    {
        var metadataText = FormatMetadata(protocolEvent.Metadata);
        return $"[{protocolEvent.Timestamp:u}] {protocolEvent.Name}: {metadataText}";
    }

    private static string FormatMetadata(IReadOnlyDictionary<string, object?> metadata)
    {
        if (metadata is null || metadata.Count == 0)
        {
            return "No metadata available.";
        }

        var parts = new List<string>(metadata.Count);
        foreach (var kvp in metadata)
        {
            var value = kvp.Value switch
            {
                null => "<null>",
                string text when string.IsNullOrWhiteSpace(text) => "<empty>",
                _ => kvp.Value.ToString()
            };
            parts.Add($"{kvp.Key}: {value}");
        }

        return string.Join(", ", parts);
    }
}
