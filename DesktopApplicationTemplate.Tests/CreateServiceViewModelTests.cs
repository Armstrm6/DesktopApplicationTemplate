using System;
using System.Collections.Generic;
using System.Linq;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;

namespace DesktopApplicationTemplate.Tests
{
    public class CreateServiceViewModelTests
    {
        [Fact]
        public void ServiceDescriptors_UsePresentationMetadata()
        {
            var descriptor = new StubDescriptor(
                "service.test",
                "Test",
                ServiceType.Tcp,
                new ServicePresentationMetadata("🧪", "#123456", "#654321", "Catalog Label"));
            var catalog = new StubCatalog(descriptor);

            var vm = new CreateServiceViewModel(catalog);

            var metadata = Assert.Single(vm.ServiceDescriptors);
            Assert.Equal(descriptor.Id, metadata.DescriptorId);
            Assert.Equal("Catalog Label", metadata.DisplayLabel);
            Assert.Equal("🧪", metadata.IconGlyph);
            Assert.Equal(ServiceType.Tcp, metadata.LegacyType);
            Assert.Equal("Test", metadata.Category);
            Assert.Null(metadata.Description);
            Assert.Equal("#123456", metadata.PrimaryAccentColor);
            Assert.Equal("#654321", metadata.SecondaryAccentColor);
            ConsoleTestLogger.LogPass();
        }

        [Fact]
        public void GenerateDefaultName_ReturnsIncrementedName()
        {
            var descriptor = new StubDescriptor(
                "service.test",
                "Test",
                ServiceType.Http,
                new ServicePresentationMetadata("🌐", null, null, "HTTP"));
            var catalog = new StubCatalog(descriptor);
            var existing = new[] { "HTTP1" };

            var vm = new CreateServiceViewModel(catalog, existing);
            var name = vm.GenerateDefaultName(descriptor.Id);

            Assert.Equal("HTTP2", name);
            ConsoleTestLogger.LogPass();
        }

        private sealed class StubCatalog : IServiceCatalog
        {
            private readonly Dictionary<string, IServiceDescriptor> _descriptorsById;
            private readonly Dictionary<ServiceType, IServiceDescriptor> _descriptorsByType;

            public StubCatalog(params IServiceDescriptor[] descriptors)
            {
                _descriptorsById = new Dictionary<string, IServiceDescriptor>(StringComparer.Ordinal);
                _descriptorsByType = new Dictionary<ServiceType, IServiceDescriptor>();
                UpdateDescriptors(descriptors);
            }

            public IReadOnlyCollection<IServiceDescriptor> Descriptors { get; private set; } = Array.Empty<IServiceDescriptor>();

            public IReadOnlyDictionary<ServiceType, string> LegacyMap { get; private set; } = new Dictionary<ServiceType, string>();

            public event EventHandler? DescriptorsChanged;

            public bool TryGetById(string id, out IServiceDescriptor descriptor) => _descriptorsById.TryGetValue(id, out descriptor!);

            public bool TryGetByLegacyType(ServiceType legacyType, out IServiceDescriptor descriptor) => _descriptorsByType.TryGetValue(legacyType, out descriptor!);

            public void UpdateDescriptors(IEnumerable<IServiceDescriptor> descriptors)
            {
                _descriptorsById.Clear();
                _descriptorsByType.Clear();
                var snapshot = descriptors.ToArray();
                foreach (var descriptor in snapshot)
                {
                    _descriptorsById[descriptor.Id] = descriptor;
                    if (descriptor.LegacyType is { } legacy)
                    {
                        _descriptorsByType[legacy] = descriptor;
                    }
                }

                Descriptors = snapshot;
                LegacyMap = _descriptorsByType.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Id);
                DescriptorsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private sealed class StubDescriptor : IServiceDescriptor
        {
            public StubDescriptor(string id, string displayName, ServiceType legacyType, ServicePresentationMetadata presentation)
            {
                Id = id;
                DisplayName = displayName;
                LegacyType = legacyType;
                Presentation = presentation;
            }

            public string Id { get; }

            public string DisplayName { get; }

            public string Category => "Test";

            public string? Description => null;

            public ServiceType? LegacyType { get; }

            public IServiceOptionsSerializer? OptionsSerializer => null;

            public IReadOnlyCollection<ServiceFactoryBinding> Factories => Array.Empty<ServiceFactoryBinding>();

            public ServicePresentationMetadata Presentation { get; }

            public bool HasPayloadDescription => false;

            public string? DescribePayload(object? payload) => null;
        }
    }
}
