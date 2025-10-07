using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
    private readonly Dictionary<string, Dictionary<string, HashSet<MessageRoutingDirection>>> _referencesByService = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Dictionary<string, HashSet<MessageRoutingDirection>>> _referencedByService = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _referencesLock = new();
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
    public string ResolveTokens(string template, string? referencingServiceName = null)
    {
        if (template is null)
            throw new ArgumentNullException(nameof(template));

        _logger?.Log($"Resolving tokens in '{template}'", LogLevel.Debug);
        var normalizedReferencing = NormalizeServiceName(referencingServiceName);
        List<MessageRoutingReference>? references = normalizedReferencing is null ? null : new List<MessageRoutingReference>();
        var result = NewTokenRegex.Replace(template, m =>
        {
            var name = m.Groups[1].Value;
            var direction = string.Equals(m.Groups[2].Value, nameof(RoutingMessageEntry.LastInputMessage), StringComparison.Ordinal)
                ? MessageRoutingDirection.Input
                : MessageRoutingDirection.Output;

            if (references is not null)
            {
                references.Add(new MessageRoutingReference(name, direction));
            }

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

        if (normalizedReferencing is not null)
        {
            SetReferences(normalizedReferencing, references!);
        }

        _logger?.Log($"Resolved template to '{result}'", LogLevel.Debug);
        return result;
    }

    /// <inheritdoc />
    public void SetReferences(string referencingServiceName, IEnumerable<MessageRoutingReference> references)
    {
        var normalizedReferencing = NormalizeServiceName(referencingServiceName);
        if (normalizedReferencing is null)
        {
            throw new ArgumentException("Referencing service name cannot be null or whitespace.", nameof(referencingServiceName));
        }

        var referenceMap = (references ?? Array.Empty<MessageRoutingReference>())
            .Where(r => !string.IsNullOrWhiteSpace(r.ServiceName))
            .GroupBy(r => NormalizeServiceName(r.ServiceName)!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => new HashSet<MessageRoutingDirection>(g.Select(r => r.Direction)),
                StringComparer.OrdinalIgnoreCase);

        lock (_referencesLock)
        {
            if (referenceMap.Count == 0)
            {
                if (_referencesByService.Remove(normalizedReferencing, out var previous))
                {
                    foreach (var referenced in previous.Keys)
                    {
                        RemoveReferencedByEntry(referenced, normalizedReferencing);
                    }
                }
                else
                {
                    // Ensure we are removed from any referenced-by entries even if no previous entry was present.
                    foreach (var referenced in _referencedByService.Keys.ToArray())
                    {
                        RemoveReferencedByEntry(referenced, normalizedReferencing);
                    }
                }

                return;
            }

            if (!_referencesByService.TryGetValue(normalizedReferencing, out var currentReferences))
            {
                currentReferences = new Dictionary<string, HashSet<MessageRoutingDirection>>(StringComparer.OrdinalIgnoreCase);
                _referencesByService[normalizedReferencing] = currentReferences;
            }

            var toRemove = currentReferences.Keys
                .Where(key => !referenceMap.ContainsKey(key))
                .ToList();
            foreach (var removed in toRemove)
            {
                currentReferences.Remove(removed);
                RemoveReferencedByEntry(removed, normalizedReferencing);
            }

            foreach (var pair in referenceMap)
            {
                if (!currentReferences.TryGetValue(pair.Key, out var directions))
                {
                    directions = new HashSet<MessageRoutingDirection>();
                    currentReferences[pair.Key] = directions;
                }
                else
                {
                    directions.Clear();
                }

                foreach (var direction in pair.Value)
                {
                    directions.Add(direction);
                }

                if (!_referencedByService.TryGetValue(pair.Key, out var referencingMap))
                {
                    referencingMap = new Dictionary<string, HashSet<MessageRoutingDirection>>(StringComparer.OrdinalIgnoreCase);
                    _referencedByService[pair.Key] = referencingMap;
                }

                if (!referencingMap.TryGetValue(normalizedReferencing, out var referencingDirections))
                {
                    referencingDirections = new HashSet<MessageRoutingDirection>();
                    referencingMap[normalizedReferencing] = referencingDirections;
                }
                else
                {
                    referencingDirections.Clear();
                }

                foreach (var direction in pair.Value)
                {
                    referencingDirections.Add(direction);
                }
            }
        }
    }

    /// <inheritdoc />
    public IReadOnlyCollection<string> GetReferencingServices(string serviceName)
    {
        var normalized = NormalizeServiceName(serviceName);
        if (normalized is null)
        {
            return Array.Empty<string>();
        }

        lock (_referencesLock)
        {
            if (!_referencedByService.TryGetValue(normalized, out var references) || references.Count == 0)
            {
                return Array.Empty<string>();
            }

            var results = new List<string>(references.Count);
            foreach (var pair in references)
            {
                var formattedDirections = pair.Value
                    .Select(ToPropertyName)
                    .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (formattedDirections.Length == 0)
                {
                    results.Add(pair.Key);
                }
                else if (formattedDirections.Length == 1)
                {
                    results.Add($"{pair.Key}.{formattedDirections[0]}");
                }
                else
                {
                    results.Add($"{pair.Key}.{string.Join("/", formattedDirections)}");
                }
            }

            results.Sort(StringComparer.OrdinalIgnoreCase);
            return results.ToArray();
        }
    }

    private void RemoveReferencedByEntry(string referencedService, string referencingService)
    {
        if (!_referencedByService.TryGetValue(referencedService, out var referencingMap))
        {
            return;
        }

        referencingMap.Remove(referencingService);
        if (referencingMap.Count == 0)
        {
            _referencedByService.Remove(referencedService);
        }
    }

    private static string? NormalizeServiceName(string? serviceName)
    {
        return string.IsNullOrWhiteSpace(serviceName)
            ? null
            : serviceName.Trim();
    }

    private static string ToPropertyName(MessageRoutingDirection direction)
    {
        return direction == MessageRoutingDirection.Input
            ? nameof(RoutingMessageEntry.LastInputMessage)
            : nameof(RoutingMessageEntry.LastOutputMessage);
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
