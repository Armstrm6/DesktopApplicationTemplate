using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.Tests;

public static class TestHelpers
{
    public static ServiceListModel CreateService(ServiceType type, string name) => new ServiceListModel
    {
        Type = type,
        DisplayName = $"{type.ToLegacyString()} - {name}"
    };
}
