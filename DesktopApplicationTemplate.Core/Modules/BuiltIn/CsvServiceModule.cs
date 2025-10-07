using DesktopApplicationTemplate.Core.Services.Protocols.Csv;
using DesktopApplicationTemplate.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Core.Modules.BuiltIn;

/// <summary>
/// Registers the built-in CSV descriptor and services.
/// </summary>
public sealed class CsvServiceModule : BuiltInServiceModule<CsvServiceDescriptor>
{
    public override ServiceType Type => ServiceType.Csv;

    public override void RegisterServices(IServiceCollection services)
    {
        base.RegisterServices(services);
        services.AddSingleton<ICsvService, CsvService>();
    }
}
