using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Views;
using Moq;
using Xunit;

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
        var expectedPage = new Page();
        var registry = new RegistryStub(new Dictionary<ServiceType, Func<string, Page>>
        {
            { ServiceType.Mqtt, name => type == ServiceType.Mqtt && name == type.ToLegacyString() ? expectedPage : new Page() },
            { ServiceType.Ftp, name => type == ServiceType.Ftp && name == type.ToLegacyString() ? expectedPage : new Page() }
        });
        typeof(MainView).GetField("_serviceRegistry", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(view, registry);
        var providerMock = new Mock<IServiceProvider>();
        typeof(MainView).GetField("_serviceProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(view, providerMock.Object);
        typeof(MainView).GetField("_createServicePage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(view, null);
        var method = typeof(MainView).GetMethod("NavigateTo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        method.Invoke(view, new object[] { type });

        // Assert
        Assert.Equal(type, registry.LastRequestedType);
        Assert.Equal(type.ToLegacyString(), registry.LastDefaultName);
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
        var registry = new RegistryStub(new Dictionary<ServiceType, Func<string, Page>>());
        typeof(MainView).GetField("_serviceRegistry", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(view, registry);
        var providerMock = new Mock<IServiceProvider>();
        typeof(MainView).GetField("_serviceProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(view, providerMock.Object);
        var method = typeof(MainView).GetMethod("NavigateTo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        method.Invoke(view, new object[] { ServiceType.Http });

        // Assert
        var frame = (Frame)typeof(MainView).GetField("ContentFrame")!.GetValue(view)!;
        Assert.Null(frame.Content);
    }
}

file sealed class RegistryStub : IServiceUiRegistry<ServiceListModel, Page>
{
    private readonly Dictionary<ServiceType, Func<string, Page>> _navigation;

    public RegistryStub(Dictionary<ServiceType, Func<string, Page>> navigation)
    {
        _navigation = navigation;
    }

    public IReadOnlyCollection<ServiceType> SupportedServices => _navigation.Keys;

    public ServiceType? LastRequestedType { get; private set; }

    public string? LastDefaultName { get; private set; }

    public bool TryCreateService(ServiceType serviceType, IServiceProvider provider, object options, out ServiceListModel? service)
    {
        service = null;
        return false;
    }

    public bool TryCreateServicePage(ServiceType serviceType, IServiceProvider provider, out Page? page)
    {
        page = null;
        return false;
    }

    public bool TryCreateNavigationPage(ServiceType serviceType, IServiceProvider provider, string defaultName, out Page? page)
    {
        LastRequestedType = serviceType;
        LastDefaultName = defaultName;
        if (_navigation.TryGetValue(serviceType, out var factory))
        {
            page = factory(defaultName);
            return true;
        }

        page = null;
        return false;
    }
}
