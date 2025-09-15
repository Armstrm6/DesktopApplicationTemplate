using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Navigation;
using DesktopApplicationTemplate.UI.Views;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        var view = (MainView)RuntimeHelpers.GetUninitializedObject(typeof(MainView));
        typeof(MainView).GetField("ContentFrame")!.SetValue(view, new Frame());
        typeof(MainView).GetField("HomeContentGrid")!.SetValue(view, new Grid());
        var handlerMocks = new Dictionary<ServiceType, Mock<INavigationHandler>>
        {
            { ServiceType.Mqtt, new Mock<INavigationHandler>() },
            { ServiceType.Ftp, new Mock<INavigationHandler>() }
        };
        foreach (var kvp in handlerMocks)
        {
            kvp.Value.Setup(h => h.CreateView(kvp.Key.ToLegacyString())).Returns(new Page());
        }
        var expectedPage = new Page();
        handlerMocks[type].Setup(h => h.CreateView(type.ToLegacyString())).Returns(expectedPage);
        var dict = handlerMocks.ToDictionary(k => k.Key, v => v.Value.Object);
        typeof(MainView).GetField("_navigationHandlers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(view, dict);
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
        var view = (MainView)RuntimeHelpers.GetUninitializedObject(typeof(MainView));
        typeof(MainView).GetField("ContentFrame")!.SetValue(view, new Frame());
        typeof(MainView).GetField("HomeContentGrid")!.SetValue(view, new Grid());
        typeof(MainView).GetField("_navigationHandlers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(view, new Dictionary<ServiceType, INavigationHandler>());
        var method = typeof(MainView).GetMethod("NavigateTo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        method.Invoke(view, new object[] { ServiceType.Http });

        // Assert
        var frame = (Frame)typeof(MainView).GetField("ContentFrame")!.GetValue(view)!;
        Assert.Null(frame.Content);
    }
}
