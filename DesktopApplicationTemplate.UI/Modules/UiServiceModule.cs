using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace DesktopApplicationTemplate.UI.Modules;

public sealed class UiServiceModule : IServiceModule
{
    public void RegisterServices(IServiceCollection services)
    {
        // Cross-cutting UI service registrations remain in App.xaml configuration for now.
    }

    public IEnumerable<IServiceDescriptor> DescribeServices() => Array.Empty<IServiceDescriptor>();
}
