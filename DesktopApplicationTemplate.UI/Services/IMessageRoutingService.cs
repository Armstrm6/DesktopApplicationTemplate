namespace DesktopApplicationTemplate.UI.Services;

/// <summary>
/// Provides message storage and token resolution for inter-service communication.
/// </summary>
public interface IMessageRoutingService
{
    /// <summary>
    /// Updates the latest message for the specified service.
    /// </summary>
    /// <param name="serviceName">The unique name of the service.</param>
    /// <param name="message">The message to store.</param>
    void UpdateMessage(string serviceName, string message);

    /// <summary>
    /// Attempts to retrieve the latest message for the specified service.
    /// </summary>
    /// <param name="serviceName">The unique name of the service.</param>
    /// <param name="message">The message, if found.</param>
    /// <returns><c>true</c> if a message exists for the service; otherwise, <c>false</c>.</returns>
    bool TryGetMessage(string serviceName, out string? message);

    /// <summary>
    /// Resolves <c>{ServiceName.Message}</c> tokens within the provided template.
    /// </summary>
    /// <param name="template">The template containing message tokens.</param>
    /// <returns>The template with tokens replaced by their corresponding messages.</returns>
    string ResolveTokens(string template);
}
