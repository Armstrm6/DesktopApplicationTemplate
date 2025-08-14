using System;

namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Provides methods to track the latest messages per service and resolve token placeholders.
    /// </summary>
    public interface IMessageRoutingService
    {
        /// <summary>
        /// Updates the latest message for the specified service.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <param name="message">The message to store.</param>
        void UpdateMessage(string serviceName, string message);

        /// <summary>
        /// Attempts to retrieve the latest message for a service.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <param name="message">The stored message if found.</param>
        /// <returns><c>true</c> if a message is registered; otherwise, <c>false</c>.</returns>
        bool TryGetMessage(string serviceName, out string? message);

        /// <summary>
        /// Resolves <c>{ServiceName.Message}</c> tokens within the provided template.
        /// </summary>
        /// <param name="template">The template containing tokens.</param>
        /// <returns>The template with tokens replaced by registered messages.</returns>
        string ResolveTokens(string template);
    }
}
