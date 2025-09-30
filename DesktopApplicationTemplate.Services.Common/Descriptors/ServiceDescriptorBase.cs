using System;
using System.Collections.Generic;
using System.Linq;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Services.Common.Descriptors;

/// <summary>
/// Provides a reusable base implementation for service descriptors.
/// </summary>
public abstract class ServiceDescriptorBase : IServiceDescriptor
{
    protected ServiceDescriptorBase(
        string id,
        string displayName,
        string category,
        string? description,
        ServiceType legacyType,
        IServiceOptionsSerializer? optionsSerializer,
        IReadOnlyCollection<ServiceFactoryBinding>? factories)
    {
        Id = id;
        DisplayName = displayName;
        Category = category;
        Description = description;
        LegacyType = legacyType;
        OptionsSerializer = optionsSerializer;
        Factories = factories?.ToArray() ?? Array.Empty<ServiceFactoryBinding>();
    }

    public string Id { get; }

    public string DisplayName { get; }

    public string Category { get; }

    public string? Description { get; }

    public ServiceType? LegacyType { get; }

    public IServiceOptionsSerializer? OptionsSerializer { get; }

    public IReadOnlyCollection<ServiceFactoryBinding> Factories { get; }
}
