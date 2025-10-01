using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Services.Common;
using DesktopApplicationTemplate.Services.Common.Runtime;
using DesktopApplicationTemplate.Service.Services;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;

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
                services.AddServiceModules();
                services.AddCommonServices();
                services.AddOptions<HeartbeatRuntimeOptions>()
                    .BindConfiguration("Heartbeat");
                services.AddSingleton<ServiceManager>();
                services.AddHostedService<Worker>();
                services.AddFtpServer(builder => builder
                    .UseDotNetFileSystem()
                    .EnableAnonymousAuthentication());
                services.AddSingleton<IFtpServerService, FtpServerService>();
            });
        }
    }
}
