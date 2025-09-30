using System;
using System.Collections.Generic;
using System.Linq;
using DesktopApplicationTemplate.Models;

namespace DesktopApplicationTemplate.Core.Services;

/// <summary>
/// Aggregates <see cref="IServiceDescriptor"/> instances discovered from service modules.
/// </summary>
public sealed class ServiceCatalog : IServiceCatalog
{
    private readonly IReadOnlyDictionary<string, IServiceDescriptor> _descriptorsById;
    private readonly IReadOnlyDictionary<ServiceType, IServiceDescriptor> _descriptorsByLegacy;
    private readonly IReadOnlyDictionary<ServiceType, string> _legacyMap;

    public ServiceCatalog(IEnumerable<IServiceDescriptor> descriptors)
    {
        if (descriptors is null)
        {
            throw new ArgumentNullException(nameof(descriptors));
        }

        var builders = new Dictionary<string, ServiceDescriptorBuilder>(StringComparer.Ordinal);
        foreach (var descriptor in descriptors)
        {
            if (!builders.TryGetValue(descriptor.Id, out var builder))
            {
                builder = new ServiceDescriptorBuilder(descriptor.Id);
                builders.Add(descriptor.Id, builder);
            }

            builder.Merge(descriptor);
        }

        var snapshots = builders.Values.Select(b => b.Build()).ToList();

        _descriptorsById = snapshots.ToDictionary(d => d.Id, d => (IServiceDescriptor)d, StringComparer.Ordinal);
        var legacyLookup = new Dictionary<ServiceType, IServiceDescriptor>();
        foreach (var descriptor in snapshots)
        {
            if (descriptor.LegacyType is { } legacy)
            {
                if (!legacyLookup.TryAdd(legacy, descriptor))
                {
                    throw new InvalidOperationException($"Legacy service type '{legacy}' is already mapped to descriptor '{legacyLookup[legacy].Id}'.");
                }
            }
        }

        _descriptorsByLegacy = legacyLookup;
        _legacyMap = legacyLookup.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Id, StringComparer.Ordinal);
        Descriptors = snapshots;
    }

    public IReadOnlyCollection<IServiceDescriptor> Descriptors { get; }

    public IReadOnlyDictionary<ServiceType, string> LegacyMap => _legacyMap;

    public bool TryGetById(string id, out IServiceDescriptor descriptor) =>
        _descriptorsById.TryGetValue(id, out descriptor!);

    public bool TryGetByLegacyType(ServiceType legacyType, out IServiceDescriptor descriptor) =>
        _descriptorsByLegacy.TryGetValue(legacyType, out descriptor!);

    private sealed class ServiceDescriptorBuilder
    {
        private readonly string _id;
        private string? _displayName;
        private string? _category;
        private string? _description;
        private ServiceType? _legacyType;
        private IServiceOptionsSerializer? _serializer;
        private readonly Dictionary<(ServiceFactoryKind Kind, Type ContractType, string? Key), ServiceFactoryBinding> _factories = new();

        public ServiceDescriptorBuilder(string id)
        {
            _id = id;
        }

        public void Merge(IServiceDescriptor descriptor)
        {
            if (!string.Equals(_id, descriptor.Id, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Descriptor id mismatch. Expected '{_id}', received '{descriptor.Id}'.");
            }

            _displayName = AssignOrValidate(_displayName, descriptor.DisplayName, nameof(descriptor.DisplayName));
            _category = AssignOrValidate(_category, descriptor.Category, nameof(descriptor.Category));
            _description = AssignOrValidate(_description, descriptor.Description, nameof(descriptor.Description), allowNull: true);

            if (descriptor.LegacyType is { } legacy)
            {
                if (_legacyType is null)
                {
                    _legacyType = legacy;
                }
                else if (_legacyType.Value != legacy)
                {
                    throw new InvalidOperationException($"Conflicting legacy type assignments for descriptor '{_id}'.");
                }
            }

            if (descriptor.OptionsSerializer is { } serializer)
            {
                if (_serializer is null)
                {
                    _serializer = serializer;
                }
                else if (_serializer.GetType() != serializer.GetType())
                {
                    throw new InvalidOperationException($"Conflicting serializer registrations for descriptor '{_id}'.");
                }
            }

            foreach (var binding in descriptor.Factories ?? Array.Empty<ServiceFactoryBinding>())
            {
                var key = (binding.Kind, binding.ContractType, binding.Key);
                _factories[key] = binding;
            }
        }

        public IServiceDescriptor Build()
        {
            if (string.IsNullOrWhiteSpace(_displayName))
            {
                throw new InvalidOperationException($"Descriptor '{_id}' must specify a display name.");
            }

            if (string.IsNullOrWhiteSpace(_category))
            {
                throw new InvalidOperationException($"Descriptor '{_id}' must specify a category.");
            }

            var factories = _factories.Values.ToList();
            return new Snapshot(_id, _displayName!, _category!, _description, _legacyType, _serializer, factories);
        }

        private static string? AssignOrValidate(string? current, string? incoming, string propertyName, bool allowNull = false)
        {
            if (string.IsNullOrWhiteSpace(incoming))
            {
                return allowNull ? current : current ?? throw new InvalidOperationException($"Descriptor must provide a value for '{propertyName}'.");
            }

            if (current is null)
            {
                return incoming;
            }

            if (!string.Equals(current, incoming, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Conflicting values for '{propertyName}'.");
            }

            return current;
        }
    }

    private sealed class Snapshot : IServiceDescriptor
    {
        public Snapshot(
            string id,
            string displayName,
            string category,
            string? description,
            ServiceType? legacyType,
            IServiceOptionsSerializer? serializer,
            IReadOnlyCollection<ServiceFactoryBinding> factories)
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
            Description = description;
            LegacyType = legacyType;
            OptionsSerializer = serializer;
            Factories = factories;
        }

        public string Id { get; }

        public string DisplayName { get; }

        public string Category { get; }

        public string? Description { get; }

        public ServiceType? LegacyType { get; }

        public IServiceOptionsSerializer? OptionsSerializer { get; }

        public IReadOnlyCollection<ServiceFactoryBinding> Factories { get; }
    }
}
