using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Hid.Descriptors;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Services.Hid.Modules;

/// <summary>
/// Registers the HID package services and descriptor.
/// </summary>
public sealed class HidPackageModule : IServiceModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<HidServiceDescriptor>();
        services.AddSingleton<IServiceDescriptor>(sp => sp.GetRequiredService<HidServiceDescriptor>());
    }

    public IEnumerable<IServiceDescriptor> DescribeServices()
    {
        yield return new HidServiceDescriptor();
    }
}
