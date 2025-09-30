using System;
using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DesktopApplicationTemplate.Core.Tests;

public class ServiceCatalogTests
{
    [Fact]
    public void AddServiceModules_RegistersCatalogWithDescriptors()
    {
        var services = new ServiceCollection();
        services.AddServiceModules(typeof(CommonServicesModule).Assembly);

        using var provider = services.BuildServiceProvider();
        var catalog = provider.GetService<IServiceCatalog>();

        catalog.Should().NotBeNull();
        catalog!.Descriptors.Should().HaveCountGreaterThan(0);
        catalog.TryGetById(ServiceDescriptorIds.Tcp, out var tcpDescriptor).Should().BeTrue();
        tcpDescriptor!.LegacyType.Should().Be(ServiceType.Tcp);
        catalog.LegacyMap.Should().ContainKey(ServiceType.Tcp).WhoseValue.Should().Be(ServiceDescriptorIds.Tcp);
        tcpDescriptor.Presentation.IconGlyph.Should().Be("🔗");
        tcpDescriptor.Presentation.PrimaryAccentColor.Should().Be("#FFADD8E6");
        tcpDescriptor.Presentation.SecondaryAccentColor.Should().Be("#FF00008B");
        tcpDescriptor.Presentation.DisplayLabel.Should().Be("TCP");
        tcpDescriptor.HasPayloadDescription.Should().BeFalse();
        tcpDescriptor.DescribePayload(null).Should().BeNull();
    }

    [Fact]
    public void AddServiceModules_DetectsMetadataConflicts()
    {
        var services = new ServiceCollection();

        Action act = () => services.AddServiceModules(typeof(ConflictingDescriptorModule).Assembly);

        act.Should().Throw<InvalidOperationException>();
    }

    private sealed class ConflictingDescriptorModule : IServiceModule
    {
        public void RegisterServices(IServiceCollection services)
        {
        }

        public IEnumerable<IServiceDescriptor> DescribeServices()
        {
            yield return new SimpleDescriptor("test.service", "One");
            yield return new SimpleDescriptor("test.service", "Two");
        }
    }

    private sealed class SimpleDescriptor : IServiceDescriptor
    {
        public SimpleDescriptor(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }

        public string Id { get; }

        public string DisplayName { get; }

        public string Category => "Test";

        public string? Description => null;

        public ServiceType? LegacyType => null;

        public IServiceOptionsSerializer? OptionsSerializer => null;

        public IReadOnlyCollection<ServiceFactoryBinding> Factories => Array.Empty<ServiceFactoryBinding>();

        public ServicePresentationMetadata Presentation => ServicePresentationMetadata.Empty;

        public bool HasPayloadDescription => false;

        public string? DescribePayload(object? payload) => null;
    }
}
