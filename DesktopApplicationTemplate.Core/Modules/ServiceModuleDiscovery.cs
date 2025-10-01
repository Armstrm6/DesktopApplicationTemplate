using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Core.Modules;

/// <summary>
/// Provides helpers for discovering and instantiating <see cref="IServiceModule"/> implementations.
/// </summary>
public static class ServiceModuleDiscovery
{
    /// <summary>
    /// Creates module instances from the provided assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan.</param>
    /// <returns>The instantiated modules.</returns>
    public static IReadOnlyList<IServiceModule> InstantiateModules(IEnumerable<Assembly> assemblies)
    {
        if (assemblies is null)
        {
            throw new ArgumentNullException(nameof(assemblies));
        }

        var moduleType = typeof(IServiceModule);
        return assemblies
            .SelectMany(static assembly => assembly?.GetTypes() ?? Array.Empty<Type>())
            .Where(type => moduleType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
            .Select(Activator.CreateInstance)
            .OfType<IServiceModule>()
            .ToList();
    }

    /// <summary>
    /// Invokes <see cref="IServiceModule.RegisterServices(IServiceCollection)"/> for the provided modules.
    /// </summary>
    /// <param name="modules">The modules to register.</param>
    /// <param name="services">The service collection to populate.</param>
    public static void RegisterModules(IEnumerable<IServiceModule> modules, IServiceCollection services)
    {
        if (modules is null)
        {
            throw new ArgumentNullException(nameof(modules));
        }

        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        foreach (var module in modules)
        {
            module.RegisterServices(services);
        }
    }

    /// <summary>
    /// Collects descriptors exposed by the provided modules.
    /// </summary>
    /// <param name="modules">The modules to inspect.</param>
    /// <returns>The descriptors exported by the modules.</returns>
    public static IReadOnlyCollection<IServiceDescriptor> DescribeServices(IEnumerable<IServiceModule> modules)
    {
        if (modules is null)
        {
            throw new ArgumentNullException(nameof(modules));
        }

        var descriptors = new List<IServiceDescriptor>();
        foreach (var module in modules)
        {
            var moduleDescriptors = module.DescribeServices() ?? Enumerable.Empty<IServiceDescriptor>();
            descriptors.AddRange(moduleDescriptors);
        }

        return descriptors;
    }
}
