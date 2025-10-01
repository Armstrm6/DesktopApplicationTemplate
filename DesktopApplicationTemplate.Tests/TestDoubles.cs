using System;
using System.Collections.Generic;
using System.Linq;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Factories;
using DesktopApplicationTemplate.UI.Navigation;
using System.Windows.Controls;

namespace DesktopApplicationTemplate.Tests;

internal sealed class FakeServiceCatalog : IServiceCatalog
{
    private readonly Dictionary<string, IServiceDescriptor> descriptorsById = new(StringComparer.Ordinal);
    private readonly Dictionary<ServiceType, IServiceDescriptor> descriptorsByLegacy = new();

    public FakeServiceCatalog(IEnumerable<IServiceDescriptor>? descriptors = null)
    {
        UpdateDescriptors(descriptors ?? Array.Empty<IServiceDescriptor>());
    }

    public IReadOnlyCollection<IServiceDescriptor> Descriptors { get; private set; } = Array.Empty<IServiceDescriptor>();

    public IReadOnlyDictionary<ServiceType, string> LegacyMap { get; private set; } = new Dictionary<ServiceType, string>();

    public event EventHandler? DescriptorsChanged;

    public bool TryGetById(string id, out IServiceDescriptor descriptor) => descriptorsById.TryGetValue(id, out descriptor!);

    public bool TryGetByLegacyType(ServiceType legacyType, out IServiceDescriptor descriptor) => descriptorsByLegacy.TryGetValue(legacyType, out descriptor!);

    public void UpdateDescriptors(IEnumerable<IServiceDescriptor> descriptors)
    {
        descriptorsById.Clear();
        descriptorsByLegacy.Clear();
        var snapshot = descriptors.ToArray();
        foreach (var descriptor in snapshot)
        {
            descriptorsById[descriptor.Id] = descriptor;
            if (descriptor.LegacyType is { } legacy)
            {
                descriptorsByLegacy[legacy] = descriptor;
            }
        }

        Descriptors = snapshot;
        LegacyMap = descriptorsByLegacy.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Id);
        DescriptorsChanged?.Invoke(this, EventArgs.Empty);
    }

    public static FakeServiceCatalog CreateWithAllLegacyTypes()
    {
        var descriptors = Enum.GetValues<ServiceType>()
            .Select(type => new SimpleDescriptor(type.ToDescriptorId(), type.ToLegacyString(), type))
            .Cast<IServiceDescriptor>();
        return new FakeServiceCatalog(descriptors);
    }

    private sealed class SimpleDescriptor : IServiceDescriptor
    {
        public SimpleDescriptor(string id, string name, ServiceType legacyType)
        {
            Id = id;
            DisplayName = name;
            LegacyType = legacyType;
        }

        public string Id { get; }

        public string DisplayName { get; }

        public string Category => "Test";

        public string? Description => null;

        public ServiceType? LegacyType { get; }

        public IServiceOptionsSerializer? OptionsSerializer => null;

        public IReadOnlyCollection<ServiceFactoryBinding> Factories => Array.Empty<ServiceFactoryBinding>();

        public ServicePresentationMetadata Presentation => new(null, null, null, DisplayName);

        public bool HasPayloadDescription => false;

        public string? DescribePayload(object? payload) => null;
    }
}

internal sealed class FakeServiceUiRegistry : IServiceUiRegistry
{
    public FakeServiceUiRegistry(
        IReadOnlyDictionary<string, Func<Page>>? servicePages = null,
        IReadOnlyDictionary<string, Func<INavigationHandler>>? navigationHandlers = null,
        IReadOnlyDictionary<string, Func<IServiceFactory>>? factories = null,
        IReadOnlyDictionary<string, Func<IEditServiceHandler>>? editHandlers = null)
    {
        ServicePages = servicePages ?? new Dictionary<string, Func<Page>>();
        NavigationHandlers = navigationHandlers ?? new Dictionary<string, Func<INavigationHandler>>();
        Factories = factories ?? new Dictionary<string, Func<IServiceFactory>>();
        EditHandlers = editHandlers ?? new Dictionary<string, Func<IEditServiceHandler>>();
    }

    public IReadOnlyDictionary<string, Func<Page>> ServicePages { get; }

    public IReadOnlyDictionary<string, Func<INavigationHandler>> NavigationHandlers { get; }

    public IReadOnlyDictionary<string, Func<IServiceFactory>> Factories { get; }

    public IReadOnlyDictionary<string, Func<IEditServiceHandler>> EditHandlers { get; }
}
