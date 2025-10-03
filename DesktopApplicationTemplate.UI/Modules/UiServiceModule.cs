using System;
using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.UI.Modules;

public sealed class UiServiceModule : IServiceModule
{
    public ServiceType Type => throw new NotSupportedException("The UI module does not expose a service descriptor type.");

    public void RegisterServices(IServiceCollection services)
    {
        // Cross-cutting UI service registrations remain in App.xaml configuration for now.
    }

    public IEnumerable<IServiceDescriptor> DescribeServices() => Array.Empty<IServiceDescriptor>();
}
