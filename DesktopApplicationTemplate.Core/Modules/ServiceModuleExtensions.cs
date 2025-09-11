using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

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
    public static void AddServiceModules(this IServiceCollection services, params Assembly[] assemblies)
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
            .OfType<IServiceModule>();

        foreach (var module in modules)
        {
            module.RegisterServices(services);
        }
    }
}
