using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;

namespace DesktopApplicationTemplate
{
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    var env = context.HostingEnvironment.EnvironmentName ?? "Production";
                    config.SetBasePath(Directory.GetCurrentDirectory())
                          .AddJsonFile("Configuration/appsettings.json", optional: true)
                          .AddJsonFile($"Configuration/appsettings.{env}.json", optional: true)
                          .AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(context.Configuration, services);
                })
                .Build();
        }

        private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddSingleton<Views.MainWindow>();
            services.AddSingleton<Services.IStartupService, Services.StartupService>();
            services.AddSingleton<ViewModels.MainWindowViewModel>();
            services.AddSingleton<ViewModels.TcpServiceViewModel>();
            services.AddSingleton<Helpers.DependencyChecker>();

            // Load strongly typed settings
            services.Configure<Models.AppSettings>(configuration.GetSection("AppSettings"));
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost.StartAsync();

            var startupService = AppHost.Services.GetRequiredService<Services.IStartupService>();
            await startupService.RunStartupChecksAsync();

            var mainWindow = AppHost.Services.GetRequiredService<Views.MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost.StopAsync();
            AppHost.Dispose();
            base.OnExit(e);
        }
    }
}
