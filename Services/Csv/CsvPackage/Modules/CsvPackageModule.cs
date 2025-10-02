using System;
using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Csv.Descriptors;
using DesktopApplicationTemplate.Services.Csv.Options;
using DesktopApplicationTemplate.Services.Csv.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Services.Csv.Modules;

/// <summary>
/// Registers CSV package services and descriptors with the host container.
/// </summary>
public sealed class CsvPackageModule : IServiceModule
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<CsvServiceOptionsSerializer>();
        services.AddSingleton<IServiceOptionsSerializer>(sp => sp.GetRequiredService<CsvServiceOptionsSerializer>());
        services.AddSingleton<IServiceOptionsSerializer<CsvServiceOptions>>(sp => sp.GetRequiredService<CsvServiceOptionsSerializer>());

        services.AddSingleton<CsvServiceDescriptor>(sp => new CsvServiceDescriptor(sp.GetRequiredService<CsvServiceOptionsSerializer>()));
        services.AddSingleton<IServiceDescriptor>(sp => sp.GetRequiredService<CsvServiceDescriptor>());
    }

    /// <inheritdoc />
    public IEnumerable<IServiceDescriptor> DescribeServices()
    {
        yield return new CsvServiceDescriptor(new CsvServiceOptionsSerializer());
    }
}
