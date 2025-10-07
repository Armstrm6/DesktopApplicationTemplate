using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Core.Services;

/// <summary>
/// Tracks the latest messages per service and resolves token placeholders.
/// </summary>
public class MessageRoutingService : IMessageRoutingService
{
    private readonly ConcurrentDictionary<(ServiceType, string), RoutingMessageEntry> _messages = new();
    private readonly ConcurrentDictionary<string, RoutingMessageEntry> _messagesByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILoggingService? _logger;
    private static readonly Regex NewTokenRegex = new(@"\{([A-Za-z0-9_]+)\.(LastInputMessage|LastOutputMessage)\}", RegexOptions.Compiled);
    private static readonly Regex LegacyTokenRegex = new(@"\{([A-Za-z0-9_]+)\.([A-Za-z0-9_]+)\.Message\}", RegexOptions.Compiled);

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageRoutingService"/> class.
    /// </summary>
    /// <param name="logger">Optional logging service.</param>
    public MessageRoutingService(ILoggingService? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void UpdateMessage(ServiceType serviceType, string serviceName, string message, MessageRoutingDirection direction = MessageRoutingDirection.Input)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("Service name cannot be null or whitespace.", nameof(serviceName));

        var normalizedName = serviceName.Trim();
        var payload = message ?? string.Empty;

        _logger?.Log($"Updating {direction} message for {serviceType}.{normalizedName}", LogLevel.Debug);

        var entry = _messages.AddOrUpdate(
            (serviceType, normalizedName),
            _ => CreateEntry(direction, payload),
            (_, existing) =>
            {
                existing.Update(direction, payload);
                return existing;
            });

        _messagesByName.AddOrUpdate(normalizedName, _ => entry, (_, existing) =>
        {
            existing.Update(direction, payload);
            return existing;
        });

        _logger?.Log($"{direction} message for {serviceType}.{normalizedName} updated", LogLevel.Debug);
    }

    /// <inheritdoc />
    public bool TryGetMessage(ServiceType serviceType, string serviceName, MessageRoutingDirection direction, out string? message)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            message = null;
            return false;
        }

        if (_messages.TryGetValue((serviceType, serviceName.Trim()), out var entry))
        {
            message = entry.Get(direction);
            return true;
        }

        message = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetMessage(string serviceName, MessageRoutingDirection direction, out string? message)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            message = null;
            return false;
        }

        if (_messagesByName.TryGetValue(serviceName.Trim(), out var entry))
        {
            message = entry.Get(direction);
            return true;
        }

        message = null;
        return false;
    }

    /// <inheritdoc />
    public string ResolveTokens(string template)
    {
        if (template is null)
            throw new ArgumentNullException(nameof(template));

        _logger?.Log($"Resolving tokens in '{template}'", LogLevel.Debug);
        var result = NewTokenRegex.Replace(template, m =>
        {
            var name = m.Groups[1].Value;
            var direction = string.Equals(m.Groups[2].Value, nameof(RoutingMessageEntry.LastInputMessage), StringComparison.Ordinal)
                ? MessageRoutingDirection.Input
                : MessageRoutingDirection.Output;

            return TryGetMessageByName(name, direction, out var replacement)
                ? replacement
                : string.Empty;
        });

        result = LegacyTokenRegex.Replace(result, m =>
        {
            var typeStr = m.Groups[1].Value;
            var name = m.Groups[2].Value;
            if (Enum.TryParse<ServiceType>(typeStr, out var type) &&
                _messages.TryGetValue((type, name), out var entry))
            {
                return entry.LastInputMessage;
            }

            return string.Empty;
        });
        _logger?.Log($"Resolved template to '{result}'", LogLevel.Debug);
        return result;
    }

    private static RoutingMessageEntry CreateEntry(MessageRoutingDirection direction, string payload)
    {
        var entry = new RoutingMessageEntry();
        entry.Update(direction, payload);
        return entry;
    }

    private bool TryGetMessageByName(string serviceName, MessageRoutingDirection direction, out string replacement)
    {
        var key = serviceName.Trim();

        if (_messagesByName.TryGetValue(key, out var entry))
        {
            replacement = entry.Get(direction);
            return true;
        }

        replacement = string.Empty;
        return false;
    }

    private sealed class RoutingMessageEntry
    {
        private string _lastInputMessage = string.Empty;
        private string _lastOutputMessage = string.Empty;

        public string LastInputMessage => _lastInputMessage;
        public string LastOutputMessage => _lastOutputMessage;

        public void Update(MessageRoutingDirection direction, string payload)
        {
            if (direction == MessageRoutingDirection.Input)
            {
                _lastInputMessage = payload ?? string.Empty;
            }
            else
            {
                _lastOutputMessage = payload ?? string.Empty;
            }
        }

        public string Get(MessageRoutingDirection direction)
        {
            return direction == MessageRoutingDirection.Input ? _lastInputMessage : _lastOutputMessage;
        }
    }
}
