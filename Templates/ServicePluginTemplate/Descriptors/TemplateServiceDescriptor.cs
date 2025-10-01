using System;
using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Services;

namespace ServicePluginTemplate.Descriptors;

/// <summary>
/// Demonstrates how a service descriptor advertises runtime capabilities and UI metadata.
/// </summary>
public sealed class TemplateServiceDescriptor : IServiceDescriptor
{
    private static readonly IReadOnlyCollection<ServiceFactoryBinding> DescriptorFactories = new[]
    {
        ServiceFactoryBinding.Create(
            ServiceFactoryKind.Runtime,
            typeof(IServiceRuntimeFactory),
            serviceProvider => serviceProvider.GetService(typeof(Runtime.TemplateRuntimeFactory))
                ?? throw new InvalidOperationException("Template runtime factory is not registered."))
    };

    public string Id => "__DescriptorId__";

    public string DisplayName => "__PluginName__";

    public string Category => "__DescriptorCategory__";

    public string? Description => "Demonstrates how to advertise runtime services from a plug-in package.";

    public ServiceType? LegacyType => null;

    public IServiceOptionsSerializer? OptionsSerializer => null;

    public IReadOnlyCollection<ServiceFactoryBinding> Factories => DescriptorFactories;

    public ServicePresentationMetadata Presentation => new(
        IconGlyph: "\uE7F8",
        PrimaryAccentColor: "#2563EB",
        SecondaryAccentColor: "#93C5FD",
        DisplayLabel: "__PluginName__");

    public bool HasPayloadDescription => false;

    public string? DescribePayload(object? payload) => null;
}
