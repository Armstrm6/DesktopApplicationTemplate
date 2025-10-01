using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.Tests;

public static class TestHelpers
{
    public static ServiceListModel CreateService(ServiceType type, string name) => new ServiceListModel
    {
        Type = type,
        DescriptorId = type.ToDescriptorId(),
        DisplayName = $"{type.ToLegacyString()} - {name}"
    };
}
