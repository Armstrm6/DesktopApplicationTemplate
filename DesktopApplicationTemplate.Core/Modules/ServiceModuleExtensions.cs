using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DesktopApplicationTemplate.Core.Modules;

/// <summary>
/// Extension methods for registering <see cref="IServiceModule"/> implementations.
/// </summary>
public static class ServiceModuleExtensions
{
    /// <summary>
    /// Scans the provided assemblies for <see cref="IServiceModule"/> implementations and
    /// invokes their registration methods.
    /// </summary>
    /// <param name="services">The service collection to populate.</param>
    /// <param name="assemblies">Assemblies to scan. If none are provided, the current app domain assemblies are used.</param>
    public static IServiceCatalog AddServiceModules(this IServiceCollection services, params Assembly[] assemblies)
    {
        assemblies ??= Array.Empty<Assembly>();
        if (assemblies.Length == 0)
        {
            assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }

        var moduleType = typeof(IServiceModule);
        var modules = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => moduleType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
            .Select(t => Activator.CreateInstance(t))
            .OfType<IServiceModule>()
            .ToList();

        var descriptors = new List<IServiceDescriptor>();
        foreach (var module in modules)
        {
            module.RegisterServices(services);
            var moduleDescriptors = module.DescribeServices() ?? Enumerable.Empty<IServiceDescriptor>();
            descriptors.AddRange(moduleDescriptors);
        }

        var catalog = new ServiceCatalog(descriptors);
        services.TryAddSingleton<IServiceCatalog>(_ => catalog);
        return catalog;
    }
}
