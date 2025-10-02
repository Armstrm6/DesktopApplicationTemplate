using System;
using System.Collections.Generic;
using System.Linq;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Services;

public sealed record ServiceUiRegistration<TService, TPage>(
    ServiceType ServiceType,
    Func<IServiceProvider, object, TService> CreateService,
    Func<IServiceProvider, TPage> CreateServicePage,
    Func<IServiceProvider, string, TPage> CreateNavigationPage);

public interface IServiceUiRegistry<TService, TPage>
{
    IReadOnlyCollection<ServiceType> SupportedServices { get; }

    bool TryCreateService(ServiceType serviceType, IServiceProvider provider, object options, out TService? service);

    bool TryCreateServicePage(ServiceType serviceType, IServiceProvider provider, out TPage? page);

    bool TryCreateNavigationPage(ServiceType serviceType, IServiceProvider provider, string defaultName, out TPage? page);
}

public sealed class ServiceUiRegistry<TService, TPage> : IServiceUiRegistry<TService, TPage>
{
    private readonly IReadOnlyDictionary<ServiceType, ServiceUiRegistration<TService, TPage>> _registrations;

    public ServiceUiRegistry(IEnumerable<ServiceUiRegistration<TService, TPage>> registrations)
    {
        if (registrations is null)
        {
            throw new ArgumentNullException(nameof(registrations));
        }

        var map = new Dictionary<ServiceType, ServiceUiRegistration<TService, TPage>>();
        foreach (var registration in registrations)
        {
            map[registration.ServiceType] = registration;
        }

        _registrations = map;
        SupportedServices = map.Keys.ToArray();
    }

    public IReadOnlyCollection<ServiceType> SupportedServices { get; }

    public bool TryCreateService(ServiceType serviceType, IServiceProvider provider, object options, out TService? service)
    {
        if (!_registrations.TryGetValue(serviceType, out var registration))
        {
            service = default;
            return false;
        }

        service = registration.CreateService(provider, options);
        return true;
    }

    public bool TryCreateServicePage(ServiceType serviceType, IServiceProvider provider, out TPage? page)
    {
        if (!_registrations.TryGetValue(serviceType, out var registration))
        {
            page = default;
            return false;
        }

        page = registration.CreateServicePage(provider);
        return true;
    }

    public bool TryCreateNavigationPage(ServiceType serviceType, IServiceProvider provider, string defaultName, out TPage? page)
    {
        if (!_registrations.TryGetValue(serviceType, out var registration))
        {
            page = default;
            return false;
        }

        page = registration.CreateNavigationPage(provider, defaultName);
        return true;
    }
}
