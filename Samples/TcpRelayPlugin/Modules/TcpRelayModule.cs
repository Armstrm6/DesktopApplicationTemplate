using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using TcpRelayPlugin.Descriptors;
using TcpRelayPlugin.Runtime;

namespace TcpRelayPlugin.Modules;

public sealed class TcpRelayModule : IServiceModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<TcpRelayRuntimeFactory>();
    }

    public IEnumerable<IServiceDescriptor> DescribeServices()
    {
        yield return new TcpRelayServiceDescriptor();
    }
}
