using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Helpers;


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
            services.AddSingleton<MainView>();
            services.AddSingleton<Services.IStartupService, Services.StartupService>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<TcpServiceViewModel>();
            services.AddSingleton<DependencyChecker>();
            services.AddSingleton<HttpServiceView>();
            services.AddSingleton<HttpServiceViewModel>();
            services.AddSingleton<FileObserverView>();
            services.AddSingleton<FileObserverViewModel>();
            services.AddTransient<CreateServicePage>();
            services.AddTransient<CreateServiceViewModel>();


            // Load strongly typed settings
            services.Configure<Models.AppSettings>(configuration.GetSection("AppSettings"));
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost.StartAsync();

            var startupService = AppHost.Services.GetRequiredService<Services.IStartupService>();
            await startupService.RunStartupChecksAsync();

            var mainWindow = AppHost.Services.GetRequiredService<MainView>();
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
