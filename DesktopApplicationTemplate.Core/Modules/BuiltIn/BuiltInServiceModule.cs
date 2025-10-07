using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Core.Modules.BuiltIn;

/// <summary>
/// Provides a base implementation for registering built-in service descriptors.
/// </summary>
/// <typeparam name="TDescriptor">Descriptor type registered by the module.</typeparam>
public abstract class BuiltInServiceModule<TDescriptor> : IServiceModule
    where TDescriptor : IServiceDescriptor, new()
{
    private readonly TDescriptor _descriptor = new();

    public abstract ServiceType Type { get; }

    public virtual void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IServiceDescriptor>(_ => _descriptor);
    }

    public virtual IEnumerable<IServiceDescriptor> DescribeServices()
    {
        yield return _descriptor;
    }
}
