using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class MainViewEditServiceTests
{
    public static IEnumerable<object[]> ServiceTypes() =>
        Enum.GetValues<ServiceType>().Select(t => new object[] { t });

    [WindowsTheory]
    [MemberData(nameof(ServiceTypes))]
    public void EditService_InvokesCorrectHandler(ServiceType type)
    {
        // Arrange
        var view = (MainView)FormatterServices.GetUninitializedObject(typeof(MainView));
        var mocks = Enum.GetValues<ServiceType>()
            .ToDictionary(t => t, _ => new Mock<IEditServiceHandler>());
        var dict = mocks.ToDictionary(k => k.Key, v => v.Value.Object);
        typeof(MainView).GetField("_editHandlers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(view, dict);

        var method = typeof(MainView).GetMethod("OnEditRequested", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var service = TestHelpers.CreateService(type, "svc");

        // Act
        method.Invoke(view, new object[] { service });

        // Assert
        foreach (var kvp in mocks)
        {
            if (kvp.Key == type)
            {
                kvp.Value.Verify(h => h.Edit(service), Times.Once);
            }
            else
            {
                kvp.Value.Verify(h => h.Edit(It.IsAny<ServiceListModel>()), Times.Never);
            }
        }
    }

    [WindowsFact]
    public void EditService_UnknownType_NoHandlerCalled()
    {
        // Arrange
        var view = (MainView)FormatterServices.GetUninitializedObject(typeof(MainView));
        var mocks = Enum.GetValues<ServiceType>()
            .ToDictionary(t => t, _ => new Mock<IEditServiceHandler>());
        var dict = mocks.ToDictionary(k => k.Key, v => v.Value.Object);
        typeof(MainView).GetField("_editHandlers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(view, dict);
        var method = typeof(MainView).GetMethod("OnEditRequested", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var service = new ServiceListModel { ServiceType = (ServiceType)999, DisplayName = "Unknown" };

        // Act
        method.Invoke(view, new object[] { service });

        // Assert
        foreach (var kvp in mocks.Values)
        {
            kvp.Verify(h => h.Edit(It.IsAny<ServiceListModel>()), Times.Never);
        }
    }
}
