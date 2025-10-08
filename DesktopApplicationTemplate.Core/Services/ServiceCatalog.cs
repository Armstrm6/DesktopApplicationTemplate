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
    private readonly object _sync = new();
    private IReadOnlyDictionary<string, IServiceDescriptor> _descriptorsById = new Dictionary<string, IServiceDescriptor>(StringComparer.Ordinal);
    private IReadOnlyCollection<IServiceDescriptor> _descriptors = Array.Empty<IServiceDescriptor>();

    public ServiceCatalog(IEnumerable<IServiceDescriptor> descriptors)
    {
        UpdateDescriptors(descriptors);
    }

    public IReadOnlyCollection<IServiceDescriptor> Descriptors => _descriptors;

    public event EventHandler? DescriptorsChanged;

    public bool TryGetById(string id, out IServiceDescriptor descriptor) =>
        _descriptorsById.TryGetValue(id, out descriptor!);

    public void UpdateDescriptors(IEnumerable<IServiceDescriptor> descriptors)
    {
        if (descriptors is null)
        {
            throw new ArgumentNullException(nameof(descriptors));
        }

        lock (_sync)
        {
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

            var descriptorsById = snapshots.ToDictionary(d => d.Id, d => (IServiceDescriptor)d, StringComparer.Ordinal);

            _descriptorsById = descriptorsById;
            _descriptors = snapshots;
        }

        DescriptorsChanged?.Invoke(this, EventArgs.Empty);
    }

    private sealed class ServiceDescriptorBuilder
    {
        private readonly string _id;
        private string? _displayName;
        private string? _category;
        private string? _description;
        private ServiceType? _serviceType;
        private IServiceOptionsSerializer? _serializer;
        private readonly Dictionary<(ServiceFactoryKind Kind, Type ContractType, string? Key), ServiceFactoryBinding> _factories = new();
        private ServicePresentationMetadata? _presentation;
        private bool _hasPayloadDescriptor;
        private Func<object?, string?> _payloadDescriptor = static _ => null;

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

            if (!descriptor.Presentation.IsEmpty)
            {
                if (_presentation is null)
                {
                    _presentation = descriptor.Presentation;
                }
                else if (_presentation != descriptor.Presentation)
                {
                    throw new InvalidOperationException($"Conflicting presentation metadata for descriptor '{_id}'.");
                }
            }

            if (descriptor.HasPayloadDescription)
            {
                if (!_hasPayloadDescriptor)
                {
                    _payloadDescriptor = descriptor.DescribePayload;
                    _hasPayloadDescriptor = true;
                }
                else
                {
                    throw new InvalidOperationException($"Conflicting payload description handlers for descriptor '{_id}'.");
                }
            }

            if (descriptor.ServiceType is { } serviceType)
            {
                if (_serviceType is null)
                {
                    _serviceType = serviceType;
                }
                else if (_serviceType.Value != serviceType)
                {
                    throw new InvalidOperationException($"Conflicting service type assignments for descriptor '{_id}'.");
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
            var presentation = ServicePresentationMetadata.Normalize(_presentation);
            return new Snapshot(
                _id,
                _displayName!,
                _category!,
                _description,
                _serviceType,
                _serializer,
                factories,
                presentation,
                _hasPayloadDescriptor,
                _payloadDescriptor);
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
            ServiceType? serviceType,
            IServiceOptionsSerializer? serializer,
            IReadOnlyCollection<ServiceFactoryBinding> factories,
            ServicePresentationMetadata presentation,
            bool hasPayloadDescription,
            Func<object?, string?> payloadDescriptor)
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
            Description = description;
            ServiceType = serviceType;
            OptionsSerializer = serializer;
            Factories = factories;
            Presentation = presentation;
            HasPayloadDescription = hasPayloadDescription;
            _payloadDescriptor = payloadDescriptor;
        }

        public string Id { get; }

        public string DisplayName { get; }

        public string Category { get; }

        public string? Description { get; }

        public ServiceType? ServiceType { get; }

        public IServiceOptionsSerializer? OptionsSerializer { get; }

        public IReadOnlyCollection<ServiceFactoryBinding> Factories { get; }

        public ServicePresentationMetadata Presentation { get; }

        public bool HasPayloadDescription { get; }

        public string? DescribePayload(object? payload) => _payloadDescriptor(payload);

        private readonly Func<object?, string?> _payloadDescriptor;
    }
}
