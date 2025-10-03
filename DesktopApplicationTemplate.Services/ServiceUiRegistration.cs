using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Services;

/// <summary>
/// Describes how a service descriptor integrates with UI workflows.
/// </summary>
/// <typeparam name="TService">Type representing the UI service view model.</typeparam>
/// <typeparam name="TPage">Type representing a navigation target.</typeparam>
public sealed record ServiceUiRegistration<TService, TPage>(
    string DescriptorId,
    Func<IServiceProvider, object, TService> CreateService,
    Func<IServiceProvider, TPage> CreateServicePage,
    Func<IServiceProvider, string, TPage> CreateNavigationPage,
    ServiceType? LegacyServiceType = null,
    Action<TService, ServicePresentationMetadata>? ApplyPresentation = null);

public interface IServiceUiRegistry<TService, TPage> : IDisposable
{
    IReadOnlyCollection<ServiceType> SupportedServices { get; }

    IReadOnlyCollection<string> SupportedDescriptorIds { get; }

    bool TryCreateService(ServiceType serviceType, IServiceProvider provider, object options, out TService? service);

    bool TryCreateService(string descriptorId, IServiceProvider provider, object options, out TService? service);

    bool TryCreateServicePage(ServiceType serviceType, IServiceProvider provider, out TPage? page);

    bool TryCreateServicePage(string descriptorId, IServiceProvider provider, out TPage? page);

    bool TryCreateNavigationPage(ServiceType serviceType, IServiceProvider provider, string defaultName, out TPage? page);

    bool TryCreateNavigationPage(string descriptorId, IServiceProvider provider, string defaultName, out TPage? page);
}

/// <summary>
/// Resolves UI integrations for service descriptors and reacts to catalog updates.
/// </summary>
/// <typeparam name="TService">Type representing the UI service view model.</typeparam>
/// <typeparam name="TPage">Type representing a navigation target.</typeparam>
public sealed class ServiceUiRegistry<TService, TPage> : IServiceUiRegistry<TService, TPage>
{
    private readonly IServiceCatalog _serviceCatalog;
    private readonly IReadOnlyDictionary<string, ServiceUiRegistration<TService, TPage>> _registrations;
    private readonly Dictionary<ServiceType, string> _legacyMap = new();
    private IReadOnlyCollection<ServiceType> _supportedServices = Array.Empty<ServiceType>();
    private readonly IReadOnlyCollection<string> _supportedDescriptorIds;
    private bool _disposed;

    public ServiceUiRegistry(IServiceCatalog serviceCatalog, IEnumerable<ServiceUiRegistration<TService, TPage>> registrations)
    {
        _serviceCatalog = serviceCatalog ?? throw new ArgumentNullException(nameof(serviceCatalog));
        if (registrations is null)
        {
            throw new ArgumentNullException(nameof(registrations));
        }

        var map = new Dictionary<string, ServiceUiRegistration<TService, TPage>>(StringComparer.Ordinal);
        foreach (var registration in registrations)
        {
            if (registration is null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(registration.DescriptorId))
            {
                throw new ArgumentException("DescriptorId must be provided for all registrations.", nameof(registrations));
            }

            map[registration.DescriptorId] = registration;
            if (registration.LegacyServiceType is { } legacy)
            {
                _legacyMap[legacy] = registration.DescriptorId;
            }
        }

        _registrations = new ReadOnlyDictionary<string, ServiceUiRegistration<TService, TPage>>(map);
        _supportedDescriptorIds = _registrations.Keys.ToArray();
        UpdateLegacyMappings();
        _serviceCatalog.DescriptorsChanged += OnDescriptorsChanged;
    }

    public IReadOnlyCollection<ServiceType> SupportedServices => _supportedServices;

    public IReadOnlyCollection<string> SupportedDescriptorIds => _supportedDescriptorIds;

    public bool TryCreateService(ServiceType serviceType, IServiceProvider provider, object options, out TService? service)
    {
        if (!_legacyMap.TryGetValue(serviceType, out var descriptorId))
        {
            service = default;
            return false;
        }

        return TryCreateService(descriptorId, provider, options, out service);
    }

    public bool TryCreateService(string descriptorId, IServiceProvider provider, object options, out TService? service)
    {
        if (provider is null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        if (!_registrations.TryGetValue(descriptorId, out var registration))
        {
            service = default;
            return false;
        }

        service = registration.CreateService(provider, options);
        ApplyPresentationIfAvailable(service, registration);
        return true;
    }

    public bool TryCreateServicePage(ServiceType serviceType, IServiceProvider provider, out TPage? page)
    {
        if (!_legacyMap.TryGetValue(serviceType, out var descriptorId))
        {
            page = default;
            return false;
        }

        return TryCreateServicePage(descriptorId, provider, out page);
    }

    public bool TryCreateServicePage(string descriptorId, IServiceProvider provider, out TPage? page)
    {
        if (provider is null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        if (!_registrations.TryGetValue(descriptorId, out var registration))
        {
            page = default;
            return false;
        }

        page = registration.CreateServicePage(provider);
        return true;
    }

    public bool TryCreateNavigationPage(ServiceType serviceType, IServiceProvider provider, string defaultName, out TPage? page)
    {
        if (!_legacyMap.TryGetValue(serviceType, out var descriptorId))
        {
            page = default;
            return false;
        }

        return TryCreateNavigationPage(descriptorId, provider, defaultName, out page);
    }

    public bool TryCreateNavigationPage(string descriptorId, IServiceProvider provider, string defaultName, out TPage? page)
    {
        if (provider is null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        if (!_registrations.TryGetValue(descriptorId, out var registration))
        {
            page = default;
            return false;
        }

        page = registration.CreateNavigationPage(provider, defaultName);
        return true;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _serviceCatalog.DescriptorsChanged -= OnDescriptorsChanged;
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void OnDescriptorsChanged(object? sender, EventArgs e) => UpdateLegacyMappings();

    private void UpdateLegacyMappings()
    {
        var updated = new Dictionary<ServiceType, string>();

        foreach (var registration in _registrations.Values)
        {
            if (registration.LegacyServiceType is { } legacy)
            {
                updated[legacy] = registration.DescriptorId;
            }
        }

        foreach (var kvp in _serviceCatalog.LegacyMap)
        {
            if (_registrations.ContainsKey(kvp.Value))
            {
                updated[kvp.Key] = kvp.Value;
            }
        }

        _legacyMap.Clear();
        foreach (var kvp in updated)
        {
            _legacyMap[kvp.Key] = kvp.Value;
        }

        _supportedServices = _legacyMap.Keys.ToArray();
    }

    private void ApplyPresentationIfAvailable(TService? service, ServiceUiRegistration<TService, TPage> registration)
    {
        if (service is null || registration.ApplyPresentation is null)
        {
            return;
        }

        var presentation = _serviceCatalog.TryGetById(registration.DescriptorId, out var descriptor)
            ? descriptor.Presentation
            : ServicePresentationMetadata.Empty;

        registration.ApplyPresentation(service, presentation);
    }
}
