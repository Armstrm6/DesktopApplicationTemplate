using System;

namespace DesktopApplicationTemplate.Core.Services;

/// <summary>
/// Describes how to obtain a factory for integrating a service in a specific context.
/// </summary>
public sealed record ServiceFactoryBinding(
    ServiceFactoryKind Kind,
    Type ContractType,
    Func<IServiceProvider, object?> Resolver,
    string? Key = null)
{
    public static ServiceFactoryBinding Create(ServiceFactoryKind kind, Type contractType, Func<IServiceProvider, object?> resolver, string? key = null)
        => new(kind, contractType, resolver, key);
}
