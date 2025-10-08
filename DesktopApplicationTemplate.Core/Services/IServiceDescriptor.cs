using System.Collections.Generic;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Core.Services;

/// <summary>
/// Describes a service feature exposed by the platform.
/// </summary>
public interface IServiceDescriptor
{
    /// <summary>
    /// Gets the stable identifier for the service.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the human readable display name.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the category used for grouping similar services.
    /// </summary>
    string Category { get; }

    /// <summary>
    /// Gets an optional description for the service.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Gets the canonical <see cref="ServiceType"/> associated with the service, if any.
    /// </summary>
    ServiceType? ServiceType { get; }

    /// <summary>
    /// Gets the serializer used to persist service options, if available.
    /// </summary>
    IServiceOptionsSerializer? OptionsSerializer { get; }

    /// <summary>
    /// Gets registered factory bindings for integrating the service.
    /// </summary>
    IReadOnlyCollection<ServiceFactoryBinding> Factories { get; }

    /// <summary>
    /// Gets UI presentation metadata that can be used to render the service.
    /// </summary>
    ServicePresentationMetadata Presentation { get; }

    /// <summary>
    /// Gets a value indicating whether <see cref="DescribePayload(object?)"/> provides custom payload details.
    /// </summary>
    bool HasPayloadDescription { get; }

    /// <summary>
    /// Provides a user-friendly description for a persisted payload, if available.
    /// </summary>
    /// <param name="payload">The payload to describe.</param>
    /// <returns>The description, or <c>null</c> when not available.</returns>
    string? DescribePayload(object? payload);
}
