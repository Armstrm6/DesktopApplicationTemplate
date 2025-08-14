
using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Provides a simple mechanism for routing messages between services.
    /// </summary>
    public class MessageRoutingService
    {
        private readonly ILoggingService _logger;

        public MessageRoutingService(ILoggingService logger)
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

        /// <summary>
        /// Routes a message from a source service to a destination service.
        /// </summary>
        /// <param name="source">The originating service.</param>
        /// <param name="destination">The target service.</param>
        /// <param name="message">The message to route.</param>
        public void Route(string source, string destination, string message)
        {
            _logger.Log($"Routing message from {source} to {destination}", LogLevel.Debug);
            MessageForwarder.Forward(destination, message);
            _logger.Log("Message routed", LogLevel.Debug);
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
