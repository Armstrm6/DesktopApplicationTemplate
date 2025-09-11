using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.UI.Services;

/// <summary>
/// Tracks the latest messages per service and resolves token placeholders.
/// </summary>
public class MessageRoutingService : IMessageRoutingService
{
    private readonly ConcurrentDictionary<(ServiceType, string), string> _messages = new();
    private readonly ILoggingService? _logger;
    private static readonly Regex TokenRegex = new(@"\{([A-Za-z0-9_]+)\.([A-Za-z0-9_]+)\.Message\}", RegexOptions.Compiled);

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageRoutingService"/> class.
    /// </summary>
    /// <param name="logger">Optional logging service.</param>
    public MessageRoutingService(ILoggingService? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void UpdateMessage(ServiceType serviceType, string serviceName, string message)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("Service name cannot be null or whitespace.", nameof(serviceName));

        _logger?.Log($"Updating message for {serviceType}.{serviceName}", LogLevel.Debug);
        _messages.AddOrUpdate((serviceType, serviceName), _ => message ?? string.Empty, (_, _) => message ?? string.Empty);
        _logger?.Log($"Message for {serviceType}.{serviceName} updated", LogLevel.Debug);
    }

    /// <inheritdoc />
    public bool TryGetMessage(ServiceType serviceType, string serviceName, out string? message)
        => _messages.TryGetValue((serviceType, serviceName), out message);

    /// <inheritdoc />
    public string ResolveTokens(string template)
    {
        if (template is null)
            throw new ArgumentNullException(nameof(template));

        _logger?.Log($"Resolving tokens in '{template}'", LogLevel.Debug);
        string result = TokenRegex.Replace(template, m =>
        {
            var typeStr = m.Groups[1].Value;
            var name = m.Groups[2].Value;
            return Enum.TryParse<ServiceType>(typeStr, out var type) &&
                   _messages.TryGetValue((type, name), out var msg)
                ? msg
                : string.Empty;
        });
        _logger?.Log($"Resolved template to '{result}'", LogLevel.Debug);
        return result;
    }
}
