using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Navigation;
using DesktopApplicationTemplate.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Controls;
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
        var view = (MainView)FormatterServices.GetUninitializedObject(typeof(MainView));
        typeof(MainView).GetField("ContentFrame")!.SetValue(view, new Frame());
        typeof(MainView).GetField("HomeContentGrid")!.SetValue(view, new Grid());
        var handlerMocks = new Dictionary<ServiceType, Mock<INavigationHandler>>
        {
            { ServiceType.Mqtt, new Mock<INavigationHandler>() },
            { ServiceType.Ftp, new Mock<INavigationHandler>() }
        };
        foreach (var kvp in handlerMocks)
        {
            kvp.Value.SetupGet(h => h.ServiceType).Returns(kvp.Key);
        }
        var expectedPage = new Page();
        handlerMocks[type].Setup(h => h.CreateView(type.ToLegacyString())).Returns(expectedPage);
        var services = new ServiceCollection();
        foreach (var m in handlerMocks.Values)
            services.AddSingleton(m.Object);
        var provider = services.BuildServiceProvider();
        var host = new Mock<IHost>();
        host.Setup(h => h.Services).Returns(provider);
        typeof(App).GetProperty("AppHost")!.GetSetMethod(true)!.Invoke(null, new object[] { host.Object });
        typeof(MainView).GetField("_createServicePage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(view, null);
        var method = typeof(MainView).GetMethod("NavigateTo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        method.Invoke(view, new object[] { type });

        // Assert
        foreach (var kvp in handlerMocks)
        {
            if (kvp.Key == type)
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
        var view = (MainView)FormatterServices.GetUninitializedObject(typeof(MainView));
        typeof(MainView).GetField("ContentFrame")!.SetValue(view, new Frame());
        typeof(MainView).GetField("HomeContentGrid")!.SetValue(view, new Grid());
        var services = new ServiceCollection().BuildServiceProvider();
        var host = new Mock<IHost>();
        host.Setup(h => h.Services).Returns(services);
        typeof(App).GetProperty("AppHost")!.GetSetMethod(true)!.Invoke(null, new object[] { host.Object });
        var method = typeof(MainView).GetMethod("NavigateTo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        method.Invoke(view, new object[] { ServiceType.Http });

        // Assert
        var frame = (Frame)typeof(MainView).GetField("ContentFrame")!.GetValue(view)!;
        Assert.Null(frame.Content);
    }
}
