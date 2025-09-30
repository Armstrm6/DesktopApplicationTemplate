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
    /// Gets the legacy <see cref="ServiceType"/> associated with the service, if any.
    /// </summary>
    ServiceType? LegacyType { get; }

    /// <summary>
    /// Gets the serializer used to persist service options, if available.
    /// </summary>
    IServiceOptionsSerializer? OptionsSerializer { get; }

    /// <summary>
    /// Gets registered factory bindings for integrating the service.
    /// </summary>
    IReadOnlyCollection<ServiceFactoryBinding> Factories { get; }
}
