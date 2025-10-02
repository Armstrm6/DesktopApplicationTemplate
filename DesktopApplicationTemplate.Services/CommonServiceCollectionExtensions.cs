using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols;
using DesktopApplicationTemplate.Services.Protocols;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Services;

/// <summary>
/// Registers common service components.
/// </summary>
public static class CommonServiceCollectionExtensions
{
    /// <summary>
    /// Adds reusable service components to the container.
    /// </summary>
    public static IServiceCollection AddCommonServices(this IServiceCollection services)
    {
        services.AddSingleton<IServiceRule, ServiceRule>();
        services.AddTransient(typeof(IServiceScreen<>), typeof(ServiceScreen<>));
        services.AddSingleton<IFileSearchService, FileSearchService>();
        services.AddSingleton<IProtocolLogger, LoggingServiceProtocolLogger>();
        return services;
    }
}
