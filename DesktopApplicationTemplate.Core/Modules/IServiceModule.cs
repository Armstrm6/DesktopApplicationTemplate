using DesktopApplicationTemplate.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Core.Modules;

/// <summary>
/// Defines a module that can register services with the dependency injection container.
/// </summary>
public interface IServiceModule
{
    /// <summary>
    /// Gets the service type represented by this module.
    /// </summary>
    ServiceType Type { get; }

    /// <summary>
    /// Registers services with the provided service collection.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    void RegisterServices(IServiceCollection services);
}
