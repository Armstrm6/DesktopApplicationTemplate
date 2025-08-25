using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using DesktopApplicationTemplate.Core.Services;
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
            var builder = Host.CreateDefaultBuilder(args);
            if (runAsService && OperatingSystem.IsWindows())
            {
                builder = builder.UseWindowsService(); // Enables Windows Service behavior
            }

            return builder.ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>(); // register the background service
                services.AddSingleton<ILoggingService, LoggingService>();
                services.AddFtpServer(builder => builder
                    .UseDotNetFileSystem()
                    .EnableAnonymousAuthentication());
                services.AddSingleton<IFtpServerService, FtpServerService>();
            });
        }
    }
}
