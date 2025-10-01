using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using ServicePluginTemplate.Descriptors;
using ServicePluginTemplate.Runtime;

namespace ServicePluginTemplate.Modules;

/// <summary>
/// Registers dependencies and descriptors exposed by the sample plug-in.
/// </summary>
public sealed class TemplateServiceModule : IServiceModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<TemplateRuntimeFactory>();
    }

    public IEnumerable<IServiceDescriptor> DescribeServices()
    {
        yield return new TemplateServiceDescriptor();
    }
}
