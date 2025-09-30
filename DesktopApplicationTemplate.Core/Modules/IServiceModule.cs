using System.Collections.Generic;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApplicationTemplate.Core.Modules;

/// <summary>
/// Defines a module that can register services with the dependency injection container.
/// </summary>
public interface IServiceModule
{
    /// <summary>
    /// Registers services with the provided service collection.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    void RegisterServices(IServiceCollection services);

    /// <summary>
    /// Describes the services provided by this module for catalog registration.
    /// </summary>
    IEnumerable<IServiceDescriptor> DescribeServices();
}
