using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Services.Common;

namespace DesktopApplicationTemplate.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            bool runAsService = !args.Contains("--console");
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => logging.AddConsole().AddDebug());
            if (runAsService && OperatingSystem.IsWindows())
            {
                builder = builder.UseWindowsService(options =>
                {
                    options.ServiceName = WindowsServiceInfo.ServiceName;
                }); // Enables Windows Service behavior
            }

            return builder.ConfigureServices((hostContext, services) =>
            {
                var configuration = hostContext.Configuration;
                var loggerFactory = LoggerFactory.Create(loggingBuilder =>
                {
                    loggingBuilder
                        .AddConfiguration(configuration.GetSection("Logging"))
                        .AddDebug()
                        .AddConsole();
                });

                var pluginLoader = PluginLoader.Create(configuration, loggerFactory.CreateLogger<PluginLoader>());
                IReadOnlyCollection<Assembly> pluginAssemblies;
                try
                {
                    pluginAssemblies = pluginLoader.LoadPluginAssemblies();
                    loggerFactory
                        .CreateLogger<Program>()
                        .LogInformation(
                            "Loaded {PluginAssemblyCount} plug-in assembly(ies) from {PluginDirectory}.",
                            pluginAssemblies.Count,
                            pluginLoader.Options.RootDirectory);
                }
                catch (Exception ex)
                {
                    loggerFactory
                        .CreateLogger<Program>()
                        .LogError(
                            ex,
                            "Failed to load plug-ins from {PluginDirectory}.",
                            pluginLoader.Options.RootDirectory);
                    pluginAssemblies = Array.Empty<Assembly>();
                }
                finally
                {
                    loggerFactory.Dispose();
                }

                services.AddSingleton(pluginLoader.Options);

                var assembliesForScanning = PluginLoader.CombineWithDefaultAssemblies(pluginAssemblies);
                services.AddServiceModules(assembliesForScanning.ToArray());
                services.AddCommonServices();
                services.AddSingleton<ServiceManager>();
                services.AddHostedService<Worker>();
            });
        }
    }
}
