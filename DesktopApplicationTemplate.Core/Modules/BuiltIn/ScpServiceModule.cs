using DesktopApplicationTemplate.Core.Services.Protocols.Scp;
using DesktopApplicationTemplate.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Core.Modules.BuiltIn;

/// <summary>
/// Registers the built-in SCP descriptor and services.
/// </summary>
public sealed class ScpServiceModule : BuiltInServiceModule<ScpServiceDescriptor>
{
    public override ServiceType Type => ServiceType.Scp;

    public override void RegisterServices(IServiceCollection services)
    {
        base.RegisterServices(services);
        services.AddSingleton<IScpClientFactory, ScpClientFactory>();
        services.AddSingleton<IScpUploadService, ScpService>();
    }
}
