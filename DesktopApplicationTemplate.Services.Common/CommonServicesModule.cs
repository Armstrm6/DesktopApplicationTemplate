using System;
using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Services.Common;

/// <summary>
/// Provides shared service module hooks for common infrastructure.
/// </summary>
public sealed class CommonServicesModule : IServiceModule
{
    public void RegisterServices(IServiceCollection services)
    {
        // Shared services are registered through extension methods elsewhere.
    }

    public IEnumerable<IServiceDescriptor> DescribeServices() => Array.Empty<IServiceDescriptor>();
}
