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
    private static readonly Func<object?, string?> DefaultPayloadDescriptor = static _ => null;

    private readonly Func<object?, string?> _payloadDescriptor;

    protected ServiceDescriptorBase(
        string id,
        string displayName,
        string category,
        string? description,
        ServiceType legacyType,
        IServiceOptionsSerializer? optionsSerializer,
        IReadOnlyCollection<ServiceFactoryBinding>? factories,
        ServicePresentationMetadata? presentation = null,
        Func<object?, string?>? payloadDescription = null)
    {
        Id = id;
        DisplayName = displayName;
        Category = category;
        Description = description;
        LegacyType = legacyType;
        OptionsSerializer = optionsSerializer;
        Factories = factories?.ToArray() ?? Array.Empty<ServiceFactoryBinding>();
        Presentation = ServicePresentationMetadata.Normalize(presentation);
        _payloadDescriptor = payloadDescription ?? DefaultPayloadDescriptor;
        HasPayloadDescription = payloadDescription is not null;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public string Category { get; }

    public string? Description { get; }

    public ServiceType? LegacyType { get; }

    public IServiceOptionsSerializer? OptionsSerializer { get; }

    public IReadOnlyCollection<ServiceFactoryBinding> Factories { get; }

    public ServicePresentationMetadata Presentation { get; }

    public bool HasPayloadDescription { get; }

    public virtual string? DescribePayload(object? payload) => _payloadDescriptor(payload);
}
