using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Http.Descriptors;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Services.Http.Modules;

public sealed class HttpPackageModule : IServiceModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<HttpServiceDescriptor>();
        services.AddSingleton<IServiceDescriptor>(sp => sp.GetRequiredService<HttpServiceDescriptor>());
    }

    public IEnumerable<IServiceDescriptor> DescribeServices()
    {
        yield return new HttpServiceDescriptor();
    }
}
