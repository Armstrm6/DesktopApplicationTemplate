using System.Collections.Generic;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Core.Services;

/// <summary>
/// Provides lookup access to registered <see cref="IServiceDescriptor"/> instances.
/// </summary>
public interface IServiceCatalog
{
    /// <summary>
    /// Gets the descriptors registered with the catalog.
    /// </summary>
    IReadOnlyCollection<IServiceDescriptor> Descriptors { get; }

    /// <summary>
    /// Gets a mapping of legacy <see cref="ServiceType"/> values to descriptor identifiers.
    /// </summary>
    IReadOnlyDictionary<ServiceType, string> LegacyMap { get; }

    /// <summary>
    /// Attempts to retrieve a descriptor by its unique identifier.
    /// </summary>
    bool TryGetById(string id, out IServiceDescriptor descriptor);

    /// <summary>
    /// Attempts to retrieve a descriptor from a legacy <see cref="ServiceType"/>.
    /// </summary>
    bool TryGetByLegacyType(ServiceType legacyType, out IServiceDescriptor descriptor);
}
