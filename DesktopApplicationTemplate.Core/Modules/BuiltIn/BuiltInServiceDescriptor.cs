using System;
using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Serialization;
using DesktopApplicationTemplate.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Core.Modules.BuiltIn;

/// <summary>
/// Provides a base implementation for built-in <see cref="IServiceDescriptor"/> entries.
/// </summary>
/// <typeparam name="TOptions">Options type serialized for the service.</typeparam>
public abstract class BuiltInServiceDescriptor<TOptions> : IServiceDescriptor
    where TOptions : class, new()
{
    private readonly ServicePresentationMetadata _presentation;
    private readonly IReadOnlyCollection<ServiceFactoryBinding> _factories;
    private readonly IServiceOptionsSerializer _serializer;

    protected BuiltInServiceDescriptor(
        string id,
        string displayName,
        string category,
        ServiceType serviceType,
        ServicePresentationMetadata presentation,
        string? description = null,
        IServiceOptionsSerializer? serializer = null,
        IReadOnlyCollection<ServiceFactoryBinding>? factories = null)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        Category = category ?? throw new ArgumentNullException(nameof(category));
        ServiceType = serviceType;
        Description = description;
        _presentation = ServicePresentationMetadata.Normalize(presentation);
        _serializer = serializer ?? new JsonServiceOptionsSerializer<TOptions>();

        _factories = factories ??
        [
            ServiceFactoryBinding.Create(
                ServiceFactoryKind.Runtime,
                typeof(NoOpServiceRuntimeFactory),
                provider => provider.GetRequiredService<NoOpServiceRuntimeFactory>()),
            ServiceFactoryBinding.Create(
                ServiceFactoryKind.UserInterface,
                typeof(ServicePresentationMetadata),
                _ => _presentation)
        ];
    }

    public string Id { get; }

    public string DisplayName { get; }

    public string Category { get; }

    public string? Description { get; }

    public ServiceType? ServiceType { get; }

    public IServiceOptionsSerializer OptionsSerializer => _serializer;

    public IReadOnlyCollection<ServiceFactoryBinding> Factories => _factories;

    public ServicePresentationMetadata Presentation => _presentation;

    public bool HasPayloadDescription => false;

    public virtual string? DescribePayload(object? payload) => null;
}
