using System;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.UI.Configuration;

/// <summary>
/// Describes how a UI component participates in a service descriptor workflow.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class ServiceDescriptorRegistrationAttribute : Attribute
{
    public ServiceDescriptorRegistrationAttribute(string descriptorId, ServiceRegistrationKind kind, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        DescriptorId = descriptorId ?? throw new ArgumentNullException(nameof(descriptorId));
        Kind = kind;
        Lifetime = lifetime;
    }

    /// <summary>
    /// Gets the identifier of the descriptor associated with the target type.
    /// </summary>
    public string DescriptorId { get; }

    /// <summary>
    /// Gets the registration category for the target type.
    /// </summary>
    public ServiceRegistrationKind Kind { get; }

    /// <summary>
    /// Gets the DI lifetime used when registering the target type.
    /// </summary>
    public ServiceLifetime Lifetime { get; }
}

/// <summary>
/// Enumerates the kinds of UI registrations associated with a service descriptor.
/// </summary>
public enum ServiceRegistrationKind
{
    CreateView,
    CreateViewModel,
    EditView,
    EditViewModel,
    AdvancedView,
    AdvancedViewModel,
    SupplementalView,
    SupplementalViewModel,
    NavigationHandler,
    EditHandler,
    ServiceFactory,
    ServicePage,
    ServicePageViewModel
}
