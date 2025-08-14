using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.Services
{
    public interface IMessageRoutingService
    {
        void UpdateMessage(string serviceName, string message);
        bool TryGetMessage(string serviceName, out string? message);
        string ResolveTokens(string template);
    }

    /// <summary>
    /// Tracks latest messages per service and resolves token placeholders.
    /// </summary>
    public class MessageRoutingService : IMessageRoutingService
    {
        private readonly ConcurrentDictionary<string, string> _messages = new();
        private readonly ILoggingService? _logger;
        private static readonly Regex TokenRegex = new(@"\{([A-Za-z0-9_]+)\.Message\}", RegexOptions.Compiled);

        public MessageRoutingService(ILoggingService? logger = null)
        {
            _logger = logger;
        }

        public void UpdateMessage(string serviceName, string message)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("Service name cannot be null or whitespace.", nameof(serviceName));

            _messages[serviceName] = message ?? string.Empty;
            _logger?.Log($"Updated message for {serviceName}", LogLevel.Debug);
        }

        public bool TryGetMessage(string serviceName, out string? message)
            => _messages.TryGetValue(serviceName, out message);

        public string ResolveTokens(string template)
        {
            if (template is null)
                throw new ArgumentNullException(nameof(template));

            string result = TokenRegex.Replace(template, m =>
            {
                var name = m.Groups[1].Value;
                return _messages.TryGetValue(name, out var msg) ? msg : string.Empty;
            });

            _logger?.Log($"Resolved template '{template}' to '{result}'", LogLevel.Debug);
            return result;
        }
    }
}

