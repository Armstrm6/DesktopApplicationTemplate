using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Navigation;
using DesktopApplicationTemplate.UI.Views;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Xunit;
using DesktopApplicationTemplate.Services.Common.Descriptors;
using DesktopApplicationTemplate.UI.Factories;
using DesktopApplicationTemplate.UI.EditHandlers;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Serialization;

namespace DesktopApplicationTemplate.Tests;

public class MainViewCreateNavigationTests
{
    public static IEnumerable<object[]> NavigationTypes() => new[]
    {
        new object[] { ServiceType.Mqtt },
        new object[] { ServiceType.Ftp }
    };

    [WindowsTheory]
    [MemberData(nameof(NavigationTypes))]
    public void NavigateTo_InvokesCorrectHandler(ServiceType type)
    {
        // Arrange
        var view = (MainView)RuntimeHelpers.GetUninitializedObject(typeof(MainView));
        typeof(MainView).GetField("ContentFrame")!.SetValue(view, new Frame());
        typeof(MainView).GetField("HomeContentGrid")!.SetValue(view, new Grid());
        var handlerMocks = new Dictionary<string, Mock<INavigationHandler>>
        {
            { ServiceDescriptorIds.Mqtt, new Mock<INavigationHandler>() },
            { ServiceDescriptorIds.Ftp, new Mock<INavigationHandler>() }
        };
        foreach (var kvp in handlerMocks)
        {
            kvp.Value.SetupGet(h => h.DescriptorId).Returns(kvp.Key);
            kvp.Value.Setup(h => h.CreateView(It.IsAny<string>())).Returns(new Page());
        }
        var expectedPage = new Page();
        var targetDescriptor = type == ServiceType.Mqtt ? ServiceDescriptorIds.Mqtt : ServiceDescriptorIds.Ftp;
        handlerMocks[targetDescriptor].Setup(h => h.CreateView(type.ToLegacyString())).Returns(expectedPage);
        var registry = new StubRegistry(
            navigationHandlers: handlerMocks.ToDictionary(k => k.Key, v => new Func<INavigationHandler>(() => v.Value.Object)));
        var catalog = new StubCatalog(new Dictionary<ServiceType, string>
        {
            { ServiceType.Mqtt, ServiceDescriptorIds.Mqtt },
            { ServiceType.Ftp, ServiceDescriptorIds.Ftp }
        });
        typeof(MainView).GetField("_uiRegistry", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)! 
            .SetValue(view, registry);
        typeof(MainView).GetField("_catalog", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)! 
            .SetValue(view, catalog);
        typeof(MainView).GetField("_createServicePage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)! 
            .SetValue(view, null);
        var method = typeof(MainView).GetMethod("NavigateTo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        method.Invoke(view, new object[] { targetDescriptor });

        // Assert
        foreach (var kvp in handlerMocks)
        {
            if (kvp.Key == targetDescriptor)
            {
                kvp.Value.Verify(h => h.CreateView(type.ToLegacyString()), Times.Once);
            }
            else
            {
                kvp.Value.Verify(h => h.CreateView(It.IsAny<string>()), Times.Never);
            }
        }
        var frame = (Frame)typeof(MainView).GetField("ContentFrame")!.GetValue(view)!;
        Assert.Same(expectedPage, frame.Content);
    }

    [WindowsFact]
    public void NavigateTo_NoHandler_DoesNothing()
    {
        // Arrange
        var view = (MainView)RuntimeHelpers.GetUninitializedObject(typeof(MainView));
        typeof(MainView).GetField("ContentFrame")!.SetValue(view, new Frame());
        typeof(MainView).GetField("HomeContentGrid")!.SetValue(view, new Grid());
        var registry = new StubRegistry(navigationHandlers: new Dictionary<string, Func<INavigationHandler>>());
        var catalog = new StubCatalog(new Dictionary<ServiceType, string>());
        typeof(MainView).GetField("_uiRegistry", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(view, registry);
        typeof(MainView).GetField("_catalog", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(view, catalog);
        var method = typeof(MainView).GetMethod("NavigateTo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        method.Invoke(view, new object[] { ServiceDescriptorIds.Http });

        // Assert
        var frame = (Frame)typeof(MainView).GetField("ContentFrame")!.GetValue(view)!;
        Assert.Null(frame.Content);
    }
}

internal sealed class StubRegistry : IServiceUiRegistry
{
    public StubRegistry(
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

internal sealed class StubCatalog : IServiceCatalog
{
    private readonly Dictionary<string, IServiceDescriptor> _descriptorsById;
    private readonly Dictionary<ServiceType, IServiceDescriptor> _descriptorsByType;

    public StubCatalog(Dictionary<ServiceType, string> legacyMap)
    {
        LegacyMap = legacyMap;
        _descriptorsById = legacyMap.ToDictionary(
            kvp => kvp.Value,
            kvp => (IServiceDescriptor)new StubDescriptor(kvp.Value, kvp.Key));
        _descriptorsByType = _descriptorsById.Values.ToDictionary(d => d.LegacyType!.Value);
        Descriptors = _descriptorsById.Values.ToArray();
    }

    public IReadOnlyCollection<IServiceDescriptor> Descriptors { get; }

    public IReadOnlyDictionary<ServiceType, string> LegacyMap { get; }

    public bool TryGetById(string id, out IServiceDescriptor descriptor) => _descriptorsById.TryGetValue(id, out descriptor!);

    public bool TryGetByLegacyType(ServiceType legacyType, out IServiceDescriptor descriptor) => _descriptorsByType.TryGetValue(legacyType, out descriptor!);

    private sealed class StubDescriptor : IServiceDescriptor
    {
        public StubDescriptor(string id, ServiceType legacy)
        {
            Id = id;
            LegacyType = legacy;
        }

        public string Id { get; }

        public string DisplayName => Id;

        public string Category => "Test";

        public string? Description => null;

        public ServiceType? LegacyType { get; }

        public IServiceOptionsSerializer? OptionsSerializer => null;

        public IReadOnlyCollection<ServiceFactoryBinding> Factories => Array.Empty<ServiceFactoryBinding>();

        public ServicePresentationMetadata Presentation => ServicePresentationMetadata.Empty;

        public bool HasPayloadDescription => false;

        public string? DescribePayload(object? payload) => null;
    }
}
