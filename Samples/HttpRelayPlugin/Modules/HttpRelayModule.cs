using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using HttpRelayPlugin.Descriptors;
using HttpRelayPlugin.Runtime;

namespace HttpRelayPlugin.Modules;

public sealed class HttpRelayModule : IServiceModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<HttpRelayRuntimeFactory>();
    }

    public IEnumerable<IServiceDescriptor> DescribeServices()
    {
        yield return new HttpRelayServiceDescriptor();
    }
}
