using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Configuration;
using DesktopApplicationTemplate.UI.EditHandlers;
using DesktopApplicationTemplate.UI.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.UI.Navigation;

public interface IServiceUiRegistry
{
    IReadOnlyDictionary<string, Func<Page>> ServicePages { get; }
    IReadOnlyDictionary<string, Func<INavigationHandler>> NavigationHandlers { get; }
    IReadOnlyDictionary<string, Func<IServiceFactory>> Factories { get; }
    IReadOnlyDictionary<string, Func<IEditServiceHandler>> EditHandlers { get; }
}

internal sealed class ServiceUiRegistry : IServiceUiRegistry
{
    private ServiceUiRegistry(
        IReadOnlyDictionary<string, Func<Page>> servicePages,
        IReadOnlyDictionary<string, Func<INavigationHandler>> navigationHandlers,
        IReadOnlyDictionary<string, Func<IServiceFactory>> factories,
        IReadOnlyDictionary<string, Func<IEditServiceHandler>> editHandlers)
    {
        ServicePages = servicePages;
        NavigationHandlers = navigationHandlers;
        Factories = factories;
        EditHandlers = editHandlers;
    }

    public IReadOnlyDictionary<string, Func<Page>> ServicePages { get; }

    public IReadOnlyDictionary<string, Func<INavigationHandler>> NavigationHandlers { get; }

    public IReadOnlyDictionary<string, Func<IServiceFactory>> Factories { get; }

    public IReadOnlyDictionary<string, Func<IEditServiceHandler>> EditHandlers { get; }

    public static ServiceUiRegistry Create(IServiceProvider services, IServiceCatalog catalog, IReadOnlyCollection<ServiceRegistrationInfo> registrations)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (catalog is null)
        {
            throw new ArgumentNullException(nameof(catalog));
        }

        if (registrations is null)
        {
            throw new ArgumentNullException(nameof(registrations));
        }

        var descriptorIds = new HashSet<string>(catalog.Descriptors.Select(d => d.Id), StringComparer.Ordinal);

        var navigationHandlers = BuildFactoryDictionary<INavigationHandler>(services, registrations, descriptorIds, ServiceRegistrationKind.NavigationHandler);
        var editHandlers = BuildFactoryDictionary<IEditServiceHandler>(services, registrations, descriptorIds, ServiceRegistrationKind.EditHandler);
        var factories = BuildFactoryDictionary<IServiceFactory>(services, registrations, descriptorIds, ServiceRegistrationKind.ServiceFactory);
        var pages = BuildFactoryDictionary<Page>(services, registrations, descriptorIds, ServiceRegistrationKind.ServicePage);

        return new ServiceUiRegistry(pages, navigationHandlers, factories, editHandlers);
    }

    private static IReadOnlyDictionary<string, Func<TContract>> BuildFactoryDictionary<TContract>(
        IServiceProvider services,
        IEnumerable<ServiceRegistrationInfo> registrations,
        HashSet<string> descriptorIds,
        ServiceRegistrationKind kind)
    {
        var comparer = StringComparer.Ordinal;
        var result = new Dictionary<string, Func<TContract>>(comparer);

        foreach (var registration in registrations.Where(r => r.Kind == kind && descriptorIds.Contains(r.DescriptorId)))
        {
            if (result.ContainsKey(registration.DescriptorId))
            {
                continue;
            }

            result[registration.DescriptorId] = () => (TContract)services.GetRequiredService(registration.ImplementationType);
        }

        return result;
    }
}

internal sealed record ServiceRegistrationInfo(
    string DescriptorId,
    Type ImplementationType,
    ServiceRegistrationKind Kind,
    Microsoft.Extensions.DependencyInjection.ServiceLifetime Lifetime);
