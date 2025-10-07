using System.Collections.Generic;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Core.Services;

/// <summary>
/// Provides message storage and token resolution for inter-service communication.
/// </summary>
public interface IMessageRoutingService
{
    /// <summary>
    /// Updates the latest message for the specified service.
    /// </summary>
    /// <param name="serviceType">The category of the service.</param>
    /// <param name="serviceName">The unique name of the service.</param>
    /// <param name="message">The message to store.</param>
    void UpdateMessage(ServiceType serviceType, string serviceName, string message, MessageRoutingDirection direction = MessageRoutingDirection.Input);

    /// <summary>
    /// Attempts to retrieve the latest message for the specified service.
    /// </summary>
    /// <param name="serviceType">The category of the service.</param>
    /// <param name="serviceName">The unique name of the service.</param>
    /// <param name="message">The message, if found.</param>
    /// <returns><c>true</c> if a message exists for the service; otherwise, <c>false</c>.</returns>
    bool TryGetMessage(ServiceType serviceType, string serviceName, MessageRoutingDirection direction, out string? message);

    /// <summary>
    /// Attempts to retrieve the latest message by service name without requiring the service type.
    /// </summary>
    /// <param name="serviceName">The unique name of the service.</param>
    /// <param name="direction">Specifies whether to return the last input or output message.</param>
    /// <param name="message">The resolved message, if available.</param>
    /// <returns><c>true</c> when a message is available; otherwise, <c>false</c>.</returns>
    bool TryGetMessage(string serviceName, MessageRoutingDirection direction, out string? message);

    /// <summary>
    /// Replaces <c>{ServiceName.LastInputMessage}</c> and <c>{ServiceName.LastOutputMessage}</c> tokens within the provided template.
    /// </summary>
    /// <param name="template">The template containing message tokens.</param>
    /// <param name="referencingServiceName">
    /// Optional service name that owns the template. When provided, routing dependencies are refreshed to
    /// reflect the referenced services discovered in <paramref name="template"/>.
    /// </param>
    /// <returns>The template with tokens replaced by their corresponding messages.</returns>
    string ResolveTokens(string template, string? referencingServiceName = null);

    /// <summary>
    /// Updates the routing dependencies for the specified service.
    /// </summary>
    /// <param name="referencingServiceName">The service that references other routed messages.</param>
    /// <param name="references">The set of services and message directions referenced by the service.</param>
    void SetReferences(string referencingServiceName, IEnumerable<MessageRoutingReference> references);

    /// <summary>
    /// Gets the services that currently reference the specified service's routed messages.
    /// </summary>
    /// <param name="serviceName">The name of the service being referenced.</param>
    /// <returns>A read-only collection of service references formatted as <c>ServiceName.Property</c>.</returns>
    IReadOnlyCollection<string> GetReferencingServices(string serviceName);
}

/// <summary>
/// Describes a routed message dependency from one service to another.
/// </summary>
/// <param name="ServiceName">The referenced service name.</param>
/// <param name="Direction">The message direction requested from the referenced service.</param>
public readonly record struct MessageRoutingReference(string ServiceName, MessageRoutingDirection Direction);
