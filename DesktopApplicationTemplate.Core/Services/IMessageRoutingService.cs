using System;
using System.Collections.Generic;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Core.Services;

/// <summary>
/// Provides message storage and token resolution for inter-service communication.
/// </summary>
public interface IMessageRoutingService
{
    /// <summary>
    /// Raised when a routed service attribute changes.
    /// </summary>
    event EventHandler<ServiceAttributeChangedEventArgs>? AttributeChanged;

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
    /// Publishes or updates an attribute value for the specified service.
    /// </summary>
    /// <param name="serviceType">The category of the service.</param>
    /// <param name="serviceName">The unique name of the service.</param>
    /// <param name="attributeName">The attribute to publish.</param>
    /// <param name="value">The attribute value.</param>
    void PublishAttribute(ServiceType serviceType, string serviceName, string attributeName, string? value);

    /// <summary>
    /// Publishes or updates an attribute value for the specified service without requiring a service type.
    /// </summary>
    /// <param name="serviceName">The unique name of the service.</param>
    /// <param name="attributeName">The attribute to publish.</param>
    /// <param name="value">The attribute value.</param>
    void PublishAttribute(string serviceName, string attributeName, string? value);

    /// <summary>
    /// Attempts to retrieve the value of a named attribute for the specified service.
    /// </summary>
    /// <param name="serviceType">The category of the service.</param>
    /// <param name="serviceName">The unique name of the service.</param>
    /// <param name="attributeName">The attribute to resolve.</param>
    /// <param name="value">When found, the attribute value.</param>
    /// <returns><c>true</c> when the attribute exists; otherwise, <c>false</c>.</returns>
    bool TryGetAttribute(ServiceType serviceType, string serviceName, string attributeName, out string? value);

    /// <summary>
    /// Attempts to retrieve the value of a named attribute for the specified service without requiring a service type.
    /// </summary>
    /// <param name="serviceName">The unique name of the service.</param>
    /// <param name="attributeName">The attribute to resolve.</param>
    /// <param name="value">When found, the attribute value.</param>
    /// <returns><c>true</c> when the attribute exists; otherwise, <c>false</c>.</returns>
    bool TryGetAttribute(string serviceName, string attributeName, out string? value);

    /// <summary>
    /// Clears a published attribute for the specified service.
    /// </summary>
    /// <param name="serviceType">The category of the service.</param>
    /// <param name="serviceName">The unique name of the service.</param>
    /// <param name="attributeName">The attribute to clear.</param>
    void ClearAttribute(ServiceType serviceType, string serviceName, string attributeName);

    /// <summary>
    /// Clears a published attribute for the specified service without requiring a service type.
    /// </summary>
    /// <param name="serviceName">The unique name of the service.</param>
    /// <param name="attributeName">The attribute to clear.</param>
    void ClearAttribute(string serviceName, string attributeName);

    /// <summary>
    /// Clears cached messages and dependency metadata for the specified service.
    /// </summary>
    /// <param name="serviceType">The category of the service.</param>
    /// <param name="serviceName">The unique name of the service.</param>
    void ClearService(ServiceType serviceType, string serviceName);

    /// <summary>
    /// Clears cached messages and dependency metadata for services matching the supplied name.
    /// </summary>
    /// <param name="serviceName">The unique name of the service.</param>
    void ClearService(string serviceName);

    /// <summary>
    /// Replaces <c>{ServiceName.InputMessage}</c> and <c>{ServiceName.OutputMessage}</c> tokens within the provided template.
    /// Legacy <c>LastInputMessage</c> and <c>LastOutputMessage</c> tokens are still recognized for compatibility.
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
/// <param name="AttributeName">The attribute requested from the referenced service.</param>
/// <param name="Direction">The message direction associated with <paramref name="AttributeName"/>, when applicable.</param>
public readonly record struct MessageRoutingReference(string ServiceName, string AttributeName, MessageRoutingDirection? Direction = null);

/// <summary>
/// Provides event data describing a routed service attribute change.
/// </summary>
public sealed class ServiceAttributeChangedEventArgs : EventArgs
{
    public ServiceAttributeChangedEventArgs(ServiceType? serviceType, string serviceName, string attributeName, string? value, string? previousValue, bool isRemoval)
    {
        ServiceType = serviceType;
        ServiceName = serviceName;
        AttributeName = attributeName;
        Value = value;
        PreviousValue = previousValue;
        IsRemoval = isRemoval;
    }

    /// <summary>The category of the service that owns the attribute, when known.</summary>
    public ServiceType? ServiceType { get; }

    /// <summary>Gets the unique service name.</summary>
    public string ServiceName { get; }

    /// <summary>Gets the attribute name.</summary>
    public string AttributeName { get; }

    /// <summary>Gets the new attribute value, or <c>null</c> when cleared.</summary>
    public string? Value { get; }

    /// <summary>Gets the previous attribute value before the change.</summary>
    public string? PreviousValue { get; }

    /// <summary>Indicates whether the attribute was removed.</summary>
    public bool IsRemoval { get; }
}
