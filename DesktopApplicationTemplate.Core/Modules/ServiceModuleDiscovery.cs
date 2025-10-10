using System;
using System.Collections.Generic;
using System.IO;
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
    public static IReadOnlyList<IServiceModule> InstantiateModules(
        IEnumerable<Assembly> assemblies,
        Action<Assembly, IEnumerable<Exception>>? typeLoadFailureHandler = null)
    {
        if (assemblies is null)
        {
            throw new ArgumentNullException(nameof(assemblies));
        }

        var moduleType = typeof(IServiceModule);
        var modules = new List<IServiceModule>();

        foreach (var assembly in assemblies)
        {
            if (assembly is null)
            {
                continue;
            }

            var assemblyTypes = GetLoadableTypes(assembly, typeLoadFailureHandler);
            foreach (var type in assemblyTypes)
            {
                if (!moduleType.IsAssignableFrom(type) || type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                var instance = Activator.CreateInstance(type);
                if (instance is IServiceModule module)
                {
                    modules.Add(module);
                }
            }
        }

        return modules;
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

    private static IReadOnlyList<Type> GetLoadableTypes(
        Assembly assembly,
        Action<Assembly, IEnumerable<Exception>>? typeLoadFailureHandler)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            var nonNullTypes = ex.Types?.Where(static type => type is not null).Cast<Type>().ToArray()
                ?? Array.Empty<Type>();

            NotifyTypeLoadFailure(assembly, typeLoadFailureHandler, ex.LoaderExceptions, ex);
            return nonNullTypes;
        }
        catch (Exception ex) when (IsTypeLoadException(ex))
        {
            NotifyTypeLoadFailure(assembly, typeLoadFailureHandler, null, ex);
            return Array.Empty<Type>();
        }
    }

    private static void NotifyTypeLoadFailure(
        Assembly assembly,
        Action<Assembly, IEnumerable<Exception>>? typeLoadFailureHandler,
        IEnumerable<Exception?>? errors,
        Exception fallback)
    {
        if (typeLoadFailureHandler is null)
        {
            return;
        }

        var errorList = errors?
            .Where(static error => error is not null)
            .Cast<Exception>()
            .ToArray();

        if (errorList is null || errorList.Length == 0)
        {
            errorList = new[] { fallback };
        }

        typeLoadFailureHandler(assembly, errorList);
    }

    private static bool IsTypeLoadException(Exception exception)
    {
        return exception is TypeLoadException
            or FileLoadException
            or FileNotFoundException
            or BadImageFormatException;
    }
}
