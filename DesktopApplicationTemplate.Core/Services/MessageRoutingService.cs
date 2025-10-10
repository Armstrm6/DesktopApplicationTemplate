using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Core.Services;

/// <summary>
/// Tracks routed service attributes and resolves token placeholders.
/// </summary>
public class MessageRoutingService : IMessageRoutingService
{
    private readonly ConcurrentDictionary<(ServiceType, string), RoutingMessageEntry> _messages = new();
    private readonly ConcurrentDictionary<string, RoutingMessageEntry> _messagesByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Dictionary<string, HashSet<string>>> _referencesByService = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Dictionary<string, HashSet<string>>> _referencedByService = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _referencesLock = new();
    private readonly ILoggingService? _logger;
    private static readonly Regex AttributeTokenRegex = new(@"\{([A-Za-z0-9_]+)\.([A-Za-z0-9_]+)\}", RegexOptions.Compiled);
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
    public event EventHandler<ServiceAttributeChangedEventArgs>? AttributeChanged;

    /// <inheritdoc />
    public void UpdateMessage(ServiceType serviceType, string serviceName, string message, MessageRoutingDirection direction = MessageRoutingDirection.Input)
    {
        var normalizedName = NormalizeServiceName(serviceName) ?? throw new ArgumentException("Service name cannot be null or whitespace.", nameof(serviceName));
        var payload = message ?? string.Empty;

        _logger?.Log($"Updating {direction} message for {serviceType}.{normalizedName}", LogLevel.Debug);

        var attributeName = MessageRoutingAttributeHelper.FromDirection(direction);
        PublishAttributeInternal(serviceType, normalizedName, attributeName, payload);

        _logger?.Log($"{direction} message for {serviceType}.{normalizedName} updated", LogLevel.Debug);
    }

    /// <inheritdoc />
    public bool TryGetMessage(ServiceType serviceType, string serviceName, MessageRoutingDirection direction, out string? message)
    {
        var attributeName = MessageRoutingAttributeHelper.FromDirection(direction);
        if (TryGetAttribute(serviceType, serviceName, attributeName, out var value))
        {
            message = value;
            return true;
        }

        message = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetMessage(string serviceName, MessageRoutingDirection direction, out string? message)
    {
        var attributeName = MessageRoutingAttributeHelper.FromDirection(direction);
        if (TryGetAttribute(serviceName, attributeName, out var value))
        {
            message = value;
            return true;
        }

        message = null;
        return false;
    }

    /// <inheritdoc />
    public void PublishAttribute(ServiceType serviceType, string serviceName, string attributeName, string? value)
    {
        var normalizedName = NormalizeServiceName(serviceName) ?? throw new ArgumentException("Service name cannot be null or whitespace.", nameof(serviceName));
        var normalizedAttribute = MessageRoutingAttributeHelper.Normalize(attributeName, out _);
        PublishAttributeInternal(serviceType, normalizedName, normalizedAttribute, value ?? string.Empty);
    }

    /// <inheritdoc />
    public void PublishAttribute(string serviceName, string attributeName, string? value)
    {
        var normalizedName = NormalizeServiceName(serviceName) ?? throw new ArgumentException("Service name cannot be null or whitespace.", nameof(serviceName));
        var normalizedAttribute = MessageRoutingAttributeHelper.Normalize(attributeName, out _);
        PublishAttributeInternal(serviceType: null, normalizedName, normalizedAttribute, value ?? string.Empty);
    }

    /// <inheritdoc />
    public bool TryGetAttribute(ServiceType serviceType, string serviceName, string attributeName, out string? value)
    {
        var normalizedName = NormalizeServiceName(serviceName);
        if (normalizedName is null)
        {
            value = null;
            return false;
        }

        var normalizedAttribute = MessageRoutingAttributeHelper.Normalize(attributeName, out _);

        if (_messages.TryGetValue((serviceType, normalizedName), out var entry) &&
            entry.TryGetAttribute(normalizedAttribute, out var resolved))
        {
            value = resolved;
            return true;
        }

        if (_messagesByName.TryGetValue(normalizedName, out var fallback) &&
            fallback.TryGetAttribute(normalizedAttribute, out var fallbackValue))
        {
            value = fallbackValue;
            return true;
        }

        value = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetAttribute(string serviceName, string attributeName, out string? value)
    {
        var normalizedName = NormalizeServiceName(serviceName);
        if (normalizedName is null)
        {
            value = null;
            return false;
        }

        var normalizedAttribute = MessageRoutingAttributeHelper.Normalize(attributeName, out _);

        if (_messagesByName.TryGetValue(normalizedName, out var entry) &&
            entry.TryGetAttribute(normalizedAttribute, out var resolved))
        {
            value = resolved;
            return true;
        }

        value = null;
        return false;
    }

    /// <inheritdoc />
    public void ClearAttribute(ServiceType serviceType, string serviceName, string attributeName)
    {
        var normalizedName = NormalizeServiceName(serviceName) ?? throw new ArgumentException("Service name cannot be null or whitespace.", nameof(serviceName));
        var normalizedAttribute = MessageRoutingAttributeHelper.Normalize(attributeName, out _);

        if (!_messages.TryGetValue((serviceType, normalizedName), out var entry))
        {
            return;
        }

        if (!entry.RemoveAttribute(normalizedAttribute, out var previous))
        {
            return;
        }

        if (entry.IsEmpty)
        {
            _messages.TryRemove((serviceType, normalizedName), out _);
            if (_messagesByName.TryGetValue(normalizedName, out var current) && ReferenceEquals(current, entry))
            {
                var replacement = _messages.FirstOrDefault(kvp =>
                    string.Equals(kvp.Key.Item2, normalizedName, StringComparison.OrdinalIgnoreCase)).Value;

                if (replacement is null)
                {
                    _messagesByName.TryRemove(normalizedName, out _);
                }
                else
                {
                    _messagesByName[normalizedName] = replacement;
                }
            }
        }

        OnAttributeChanged(serviceType, normalizedName, normalizedAttribute, null, previous, isRemoval: true);
    }

    /// <inheritdoc />
    public void ClearAttribute(string serviceName, string attributeName)
    {
        var normalizedName = NormalizeServiceName(serviceName) ?? throw new ArgumentException("Service name cannot be null or whitespace.", nameof(serviceName));
        var normalizedAttribute = MessageRoutingAttributeHelper.Normalize(attributeName, out _);

        var notifications = new List<(ServiceType? ServiceType, string? PreviousValue)>();
        RoutingMessageEntry? removedEntry = null;
        string? removedPrevious = null;

        if (_messagesByName.TryGetValue(normalizedName, out var entry) &&
            entry.RemoveAttribute(normalizedAttribute, out var previous))
        {
            removedEntry = entry;
            removedPrevious = previous;

            if (entry.IsEmpty)
            {
                _messagesByName.TryRemove(normalizedName, out _);
            }

            notifications.Add((null, previous));
        }

        foreach (var kvp in _messages.ToArray())
        {
            if (!string.Equals(kvp.Key.Item2, normalizedName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (removedEntry is not null && ReferenceEquals(kvp.Value, removedEntry))
            {
                if (kvp.Value.IsEmpty)
                {
                    _messages.TryRemove(kvp.Key, out _);
                }

                notifications.Add((kvp.Key.Item1, removedPrevious));
                continue;
            }

            if (kvp.Value.RemoveAttribute(normalizedAttribute, out var typePrevious))
            {
                if (kvp.Value.IsEmpty)
                {
                    _messages.TryRemove(kvp.Key, out _);
                }

                notifications.Add((kvp.Key.Item1, typePrevious));
            }
        }

        if (notifications.Count == 0)
        {
            return;
        }

        var hasTyped = notifications.Any(n => n.ServiceType.HasValue);
        foreach (var (type, previous) in notifications)
        {
            if (!hasTyped || type.HasValue)
            {
                OnAttributeChanged(type, normalizedName, normalizedAttribute, null, previous, isRemoval: true);
            }
        }
    }

    /// <inheritdoc />
    public void ClearService(ServiceType serviceType, string serviceName)
    {
        var normalized = NormalizeServiceName(serviceName) ?? throw new ArgumentException("Service name cannot be null or whitespace.", nameof(serviceName));

        _logger?.Log($"Clearing routing cache for {serviceType}.{normalized}", LogLevel.Debug);
        ClearMessagesForService(normalized, serviceType);
        ClearReferencesForService(normalized);
        _logger?.Log($"Routing cache for {serviceType}.{normalized} cleared", LogLevel.Debug);
    }

    /// <inheritdoc />
    public void ClearService(string serviceName)
    {
        var normalized = NormalizeServiceName(serviceName) ?? throw new ArgumentException("Service name cannot be null or whitespace.", nameof(serviceName));

        _logger?.Log($"Clearing routing cache for {normalized}", LogLevel.Debug);
        ClearMessagesForService(normalized, serviceType: null);
        ClearReferencesForService(normalized);
        _logger?.Log($"Routing cache for {normalized} cleared", LogLevel.Debug);
    }

    /// <inheritdoc />
    public string ResolveTokens(string template, string? referencingServiceName = null)
    {
        if (template is null)
            throw new ArgumentNullException(nameof(template));

        _logger?.Log($"Resolving tokens in '{template}'", LogLevel.Debug);
        var normalizedReferencing = NormalizeServiceName(referencingServiceName);
        List<MessageRoutingReference>? references = normalizedReferencing is null ? null : new List<MessageRoutingReference>();
        var result = AttributeTokenRegex.Replace(template, m =>
        {
            var service = m.Groups[1].Value;
            var attributeToken = m.Groups[2].Value;
            var normalizedAttribute = MessageRoutingAttributeHelper.Normalize(attributeToken, out var direction);

            if (references is not null)
            {
                references.Add(new MessageRoutingReference(service, normalizedAttribute, direction));
            }

            if (direction.HasValue)
            {
                return TryGetMessageByName(service, direction.Value, out var directional)
                    ? directional
                    : string.Empty;
            }

            return TryGetAttributeByName(service, normalizedAttribute, out var resolved)
                ? resolved
                : string.Empty;
        });

        result = LegacyTokenRegex.Replace(result, m =>
        {
            var typeStr = m.Groups[1].Value;
            var name = m.Groups[2].Value;
            if (Enum.TryParse<ServiceType>(typeStr, out var type) &&
                _messages.TryGetValue((type, name), out var entry) &&
                entry.TryGetAttribute(MessageRoutingAttributeHelper.InputAttributeName, out var value))
            {
                return value;
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
        var normalizedReferencing = NormalizeServiceName(referencingServiceName) ?? throw new ArgumentException("Referencing service name cannot be null or whitespace.", nameof(referencingServiceName));

        var referenceMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var reference in references ?? Array.Empty<MessageRoutingReference>())
        {
            var normalizedService = NormalizeServiceName(reference.ServiceName);
            if (normalizedService is null || string.IsNullOrWhiteSpace(reference.AttributeName))
            {
                continue;
            }

            var normalizedAttribute = MessageRoutingAttributeHelper.Normalize(reference.AttributeName, out _);
            if (!referenceMap.TryGetValue(normalizedService, out var attributes))
            {
                attributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                referenceMap[normalizedService] = attributes;
            }

            attributes.Add(normalizedAttribute);
        }

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
                    foreach (var referenced in _referencedByService.Keys.ToArray())
                    {
                        RemoveReferencedByEntry(referenced, normalizedReferencing);
                    }
                }

                return;
            }

            if (!_referencesByService.TryGetValue(normalizedReferencing, out var currentReferences))
            {
                currentReferences = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
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
                if (!currentReferences.TryGetValue(pair.Key, out var attributes))
                {
                    attributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    currentReferences[pair.Key] = attributes;
                }
                else
                {
                    attributes.Clear();
                }

                foreach (var attribute in pair.Value)
                {
                    attributes.Add(attribute);
                }

                if (!_referencedByService.TryGetValue(pair.Key, out var referencingMap))
                {
                    referencingMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
                    _referencedByService[pair.Key] = referencingMap;
                }

                if (!referencingMap.TryGetValue(normalizedReferencing, out var referencingAttributes))
                {
                    referencingAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    referencingMap[normalizedReferencing] = referencingAttributes;
                }
                else
                {
                    referencingAttributes.Clear();
                }

                foreach (var attribute in pair.Value)
                {
                    referencingAttributes.Add(attribute);
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
                var formattedAttributes = pair.Value
                    .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (formattedAttributes.Length == 0)
                {
                    results.Add(pair.Key);
                }
                else if (formattedAttributes.Length == 1)
                {
                    results.Add($"{pair.Key}.{formattedAttributes[0]}");
                }
                else
                {
                    results.Add($"{pair.Key}.{string.Join("/", formattedAttributes)}");
                }
            }

            results.Sort(StringComparer.OrdinalIgnoreCase);
            return results.ToArray();
        }
    }

    private void PublishAttributeInternal(ServiceType? serviceType, string normalizedServiceName, string normalizedAttribute, string value)
    {
        var notifications = new List<(ServiceType? ServiceType, string? PreviousValue)>();

        if (serviceType.HasValue)
        {
            bool changed = false;
            string? previousValue = null;
            var entry = _messages.AddOrUpdate(
                (serviceType.Value, normalizedServiceName),
                _ =>
                {
                    var created = new RoutingMessageEntry();
                    changed = created.SetAttribute(normalizedAttribute, value, out previousValue);
                    return created;
                },
                (_, existing) =>
                {
                    changed = existing.SetAttribute(normalizedAttribute, value, out previousValue);
                    return existing;
                });

            _messagesByName.AddOrUpdate(normalizedServiceName, _ => entry, (_, _) => entry);

            if (changed)
            {
                notifications.Add((serviceType.Value, previousValue));
            }
        }
        else
        {
            bool changed = false;
            string? previousValue = null;
            var entry = _messagesByName.AddOrUpdate(
                normalizedServiceName,
                _ =>
                {
                    var created = new RoutingMessageEntry();
                    changed = created.SetAttribute(normalizedAttribute, value, out previousValue);
                    return created;
                },
                (_, existing) =>
                {
                    changed = existing.SetAttribute(normalizedAttribute, value, out previousValue);
                    return existing;
                });

            if (changed)
            {
                notifications.Add((null, previousValue));
            }

            foreach (var kvp in _messages.ToArray())
            {
                if (!string.Equals(kvp.Key.Item2, normalizedServiceName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (kvp.Value.SetAttribute(normalizedAttribute, value, out var typePrevious))
                {
                    notifications.Add((kvp.Key.Item1, typePrevious));
                }
            }
        }

        if (notifications.Count == 0)
        {
            return;
        }

        var hasTyped = notifications.Any(n => n.ServiceType.HasValue);
        foreach (var (type, previous) in notifications)
        {
            if (!hasTyped || type.HasValue)
            {
                OnAttributeChanged(type, normalizedServiceName, normalizedAttribute, value, previous, isRemoval: false);
            }
        }
    }

    private void ClearMessagesForService(string normalizedServiceName, ServiceType? serviceType)
    {
        if (serviceType.HasValue)
        {
            if (_messages.TryRemove((serviceType.Value, normalizedServiceName), out var entry))
            {
                if (_messagesByName.TryGetValue(normalizedServiceName, out var current) && ReferenceEquals(current, entry))
                {
                    var replacement = _messages.FirstOrDefault(kvp =>
                        string.Equals(kvp.Key.Item2, normalizedServiceName, StringComparison.OrdinalIgnoreCase)).Value;

                    if (replacement is null)
                    {
                        _messagesByName.TryRemove(normalizedServiceName, out _);
                    }
                    else
                    {
                        _messagesByName[normalizedServiceName] = replacement;
                    }
                }

                var cleared = entry.ClearAll();
                foreach (var pair in cleared)
                {
                    OnAttributeChanged(serviceType.Value, normalizedServiceName, pair.Key, null, pair.Value, isRemoval: true);
                }
            }

            return;
        }

        var affectedEntries = _messages
            .Where(kvp => string.Equals(kvp.Key.Item2, normalizedServiceName, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        foreach (var kvp in affectedEntries)
        {
            if (_messages.TryRemove(kvp.Key, out var entry))
            {
                var cleared = entry.ClearAll();
                foreach (var pair in cleared)
                {
                    OnAttributeChanged(kvp.Key.Item1, normalizedServiceName, pair.Key, null, pair.Value, isRemoval: true);
                }
            }
        }

        if (_messagesByName.TryRemove(normalizedServiceName, out var byNameEntry))
        {
            var cleared = byNameEntry.ClearAll();
            if (affectedEntries.Length == 0)
            {
                foreach (var pair in cleared)
                {
                    OnAttributeChanged(null, normalizedServiceName, pair.Key, null, pair.Value, isRemoval: true);
                }
            }
        }
    }

    private void ClearReferencesForService(string normalizedServiceName)
    {
        lock (_referencesLock)
        {
            if (_referencesByService.Remove(normalizedServiceName, out var references))
            {
                foreach (var referencedService in references.Keys.ToArray())
                {
                    RemoveReferencedByEntry(referencedService, normalizedServiceName);
                }
            }
            else
            {
                foreach (var referencedService in _referencedByService.Keys.ToArray())
                {
                    RemoveReferencedByEntry(referencedService, normalizedServiceName);
                }
            }

            if (_referencedByService.Remove(normalizedServiceName, out var referencingServices))
            {
                foreach (var referencingService in referencingServices.Keys.ToArray())
                {
                    if (_referencesByService.TryGetValue(referencingService, out var dependencies))
                    {
                        dependencies.Remove(normalizedServiceName);
                        if (dependencies.Count == 0)
                        {
                            _referencesByService.Remove(referencingService);
                        }
                    }
                }
            }
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

    private bool TryGetMessageByName(string serviceName, MessageRoutingDirection direction, out string replacement)
    {
        var attributeName = MessageRoutingAttributeHelper.FromDirection(direction);
        return TryGetAttributeByName(serviceName, attributeName, out replacement);
    }

    private bool TryGetAttributeByName(string serviceName, string attributeName, out string replacement)
    {
        var normalized = NormalizeServiceName(serviceName);
        if (normalized is null)
        {
            replacement = string.Empty;
            return false;
        }

        if (_messagesByName.TryGetValue(normalized, out var entry) &&
            entry.TryGetAttribute(attributeName, out var value))
        {
            replacement = value;
            return true;
        }

        replacement = string.Empty;
        return false;
    }

    private void OnAttributeChanged(ServiceType? serviceType, string serviceName, string attributeName, string? value, string? previousValue, bool isRemoval)
    {
        var handler = AttributeChanged;
        if (handler is null)
        {
            return;
        }

        handler.Invoke(this, new ServiceAttributeChangedEventArgs(serviceType, serviceName, attributeName, value, previousValue, isRemoval));
    }

    private static string? NormalizeServiceName(string? serviceName)
    {
        return string.IsNullOrWhiteSpace(serviceName)
            ? null
            : serviceName.Trim();
    }

    private sealed class RoutingMessageEntry
    {
        private readonly object _sync = new();
        private readonly Dictionary<string, string> _attributes = new(StringComparer.OrdinalIgnoreCase);

        public string InputMessage => GetOrDefault(MessageRoutingAttributeHelper.InputAttributeName);
        public string OutputMessage => GetOrDefault(MessageRoutingAttributeHelper.OutputAttributeName);

        public bool IsEmpty
        {
            get
            {
                lock (_sync)
                {
                    return _attributes.Count == 0;
                }
            }
        }

        public bool SetAttribute(string attributeName, string value, out string? previousValue)
        {
            var sanitized = value ?? string.Empty;
            lock (_sync)
            {
                if (_attributes.TryGetValue(attributeName, out var existing) &&
                    string.Equals(existing, sanitized, StringComparison.Ordinal))
                {
                    previousValue = existing;
                    return false;
                }

                previousValue = _attributes.TryGetValue(attributeName, out var previous) ? previous : null;
                _attributes[attributeName] = sanitized;
                return true;
            }
        }

        public bool RemoveAttribute(string attributeName, out string? previousValue)
        {
            lock (_sync)
            {
                if (_attributes.TryGetValue(attributeName, out var existing))
                {
                    _attributes.Remove(attributeName);
                    previousValue = existing;
                    return true;
                }
            }

            previousValue = null;
            return false;
        }

        public bool TryGetAttribute(string attributeName, out string value)
        {
            lock (_sync)
            {
                if (_attributes.TryGetValue(attributeName, out var existing))
                {
                    value = existing;
                    return true;
                }
            }

            value = string.Empty;
            return false;
        }

        public IReadOnlyList<KeyValuePair<string, string>> ClearAll()
        {
            lock (_sync)
            {
                if (_attributes.Count == 0)
                {
                    return Array.Empty<KeyValuePair<string, string>>();
                }

                var snapshot = _attributes.ToArray();
                _attributes.Clear();
                return snapshot;
            }
        }

        private string GetOrDefault(string attributeName)
        {
            return TryGetAttribute(attributeName, out var value) ? value : string.Empty;
        }
    }
}
