using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Common.Descriptors;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Services.Common;

/// <summary>
/// Registers shared service descriptors.
/// </summary>
public sealed class CommonServicesModule : IServiceModule
{
    public void RegisterServices(IServiceCollection services)
    {
        // Shared services are registered through extension methods elsewhere.
    }

    public IEnumerable<IServiceDescriptor> DescribeServices()
    {
        yield return new MqttServiceDescriptor();
        yield return new HttpServiceDescriptor();
        yield return new FtpServiceDescriptor();
        yield return new HidServiceDescriptor();
        yield return new CsvServiceDescriptor();
        yield return new FileObserverServiceDescriptor();
        yield return new ScpServiceDescriptor();
        yield return new TcpServiceDescriptor();
        yield return new HeartbeatServiceDescriptor();
    }
}
