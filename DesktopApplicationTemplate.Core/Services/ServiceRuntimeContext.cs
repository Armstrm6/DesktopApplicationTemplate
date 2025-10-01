using System;
using System.Collections.Generic;

namespace DesktopApplicationTemplate.Core.Services;

/// <summary>
/// Describes the runtime activation request for a service descriptor instance.
/// </summary>
public sealed class ServiceRuntimeContext
{
    public ServiceRuntimeContext(
        IServiceDescriptor descriptor,
        string descriptorId,
        string displayName,
        IReadOnlyCollection<string> associatedServices,
        object? payload)
    {
        Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        DescriptorId = descriptorId ?? throw new ArgumentNullException(nameof(descriptorId));
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        AssociatedServices = associatedServices ?? Array.Empty<string>();
        Payload = payload;
    }

    /// <summary>
    /// Gets the descriptor that provided the runtime binding.
    /// </summary>
    public IServiceDescriptor Descriptor { get; }

    /// <summary>
    /// Gets the descriptor identifier for the runtime instance.
    /// </summary>
    public string DescriptorId { get; }

    /// <summary>
    /// Gets the display name configured for the runtime instance.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets additional service associations configured for the runtime instance.
    /// </summary>
    public IReadOnlyCollection<string> AssociatedServices { get; }

    /// <summary>
    /// Gets the payload supplied by persistence, when available.
    /// </summary>
    public object? Payload { get; }
}
