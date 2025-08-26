using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.Core.Services;
// Qualify service-layer types explicitly to avoid name clashes with UI services
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;
using System.IO;
using System.Windows;


namespace DesktopApplicationTemplate.UI
{
    public partial class App : System.Windows.Application
    {
        public static IHost AppHost { get; private set; } = null!;

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
            services.AddSingleton<IStartupService, StartupService>();
            services.AddSingleton<IProcessRunner, ProcessRunner>();
            services.AddSingleton<INetworkConfigurationService, NetworkConfigurationService>();
            services.AddSingleton<NetworkConfigurationViewModel>();
            services.AddSingleton<IRichTextLogger, NullRichTextLogger>();
            services.AddSingleton<ILoggingService, LoggingService>();
            services.AddSingleton<IMessageRoutingService, MessageRoutingService>();
            services.AddSingleton<IFileDialogService, FileDialogService>();
            services.AddSingleton<SaveConfirmationHelper>();
            services.AddSingleton<CloseConfirmationHelper>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<TcpServiceView>();
            services.AddSingleton<TcpServiceViewModel>();
            services.AddSingleton<TcpServiceMessagesView>();
            services.AddTransient<TcpServiceMessagesViewModel>();
            services.AddSingleton<DependencyChecker>();
            services.AddSingleton<HttpServiceView>();
            services.AddSingleton<HttpServiceViewModel>();
            services.AddSingleton<FileObserverView>();
            services.AddSingleton<FileObserverViewModel>();
            services.AddSingleton<HeartbeatView>();
            services.AddSingleton<HeartbeatViewModel>();
            services.AddSingleton<SCPServiceView>();
            services.AddSingleton<ScpServiceViewModel>();
            services.AddSingleton<HidViewModel>();
            services.AddSingleton<HidViews>();
            services.AddSingleton<MqttService>();
            services.AddSingleton<FTPServiceView>();
            services.AddSingleton<FtpServiceViewModel>();
            services.AddFtpServer(builder => builder
                .UseDotNetFileSystem()
                .EnableAnonymousAuthentication());
            services.AddSingleton<IFtpServerService, DesktopApplicationTemplate.Service.Services.FtpServerService>();
            services.AddSingleton<CsvViewerViewModel>();
            services.AddSingleton<CsvService>();
            services.AddSingleton<CsvServiceView>();
            services.AddSingleton<SettingsViewModel>();
            services.AddTransient<SplashWindow>();
            services.AddTransient<CreateServicePage>();
            services.AddTransient<CreateServiceViewModel>();
            services.AddTransient<MqttCreateServiceView>();
            services.AddTransient<MqttCreateServiceViewModel>();
            services.AddTransient<TcpCreateServiceView>();
            services.AddTransient<TcpCreateServiceViewModel>();
            services.AddTransient<FtpServerCreateView>();
            services.AddTransient<FtpServerCreateViewModel>();
            services.AddTransient<FtpServerAdvancedConfigView>();
            services.AddTransient<FtpServerAdvancedConfigViewModel>();
            services.AddTransient<FtpServerEditView>();
            services.AddTransient<FtpServerEditViewModel>();
            services.AddTransient<HttpCreateServiceView>();
            services.AddTransient<HttpCreateServiceViewModel>();
            services.AddTransient<HttpEditServiceView>();
            services.AddTransient<HttpEditServiceViewModel>();
            services.AddTransient<HttpAdvancedConfigView>();
            services.AddTransient<HttpAdvancedConfigViewModel>();
            services.AddTransient<MqttEditConnectionView>();
            services.AddTransient<MqttEditConnectionViewModel>();
            services.AddTransient<MqttTagSubscriptionsView>();
            services.AddTransient<MqttTagSubscriptionsViewModel>();
            services.AddTransient<HidCreateServiceView>();
            services.AddTransient<HidCreateServiceViewModel>();
            services.AddTransient<HidEditServiceView>();
            services.AddTransient<HidEditServiceViewModel>();
            services.AddTransient<HidAdvancedConfigView>();
            services.AddTransient<HidAdvancedConfigViewModel>();
            services.AddTransient<SettingsPage>();


            // Load strongly typed settings
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
            services.Configure<MqttServiceOptions>(configuration.GetSection("MqttService"));
            services.Configure<TcpServiceOptions>(configuration.GetSection("TcpService"));
            services.AddOptions<DesktopApplicationTemplate.UI.Services.FtpServerOptions>()
                .BindConfiguration("FtpServer");
            services.AddOptions<HidServiceOptions>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost.StartAsync();

            var settings = AppHost.Services.GetRequiredService<SettingsViewModel>();
            settings.Load();
            Services.ThemeManager.ApplyTheme(settings.DarkTheme);

            SplashWindow? splash = null;
            if (settings.FirstRun)
            {
                splash = AppHost.Services.GetRequiredService<SplashWindow>();
                splash.Show();
            }

            var startupService = AppHost.Services.GetRequiredService<IStartupService>();
            await startupService.RunStartupChecksAsync();

            splash?.Close();
            if (splash != null)
            {
                settings.FirstRun = false;
                settings.Save();
            }

            var mainWindow = AppHost.Services.GetRequiredService<MainView>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            var vm = AppHost.Services.GetRequiredService<MainViewModel>();
            vm.SaveServices();

            var logger = AppHost.Services.GetService<Microsoft.Extensions.Logging.ILogger<App>>();
            logger?.LogInformation("Releasing keyboard state");
            Helpers.KeyboardHelper.ReleaseKeys(System.Windows.Input.Key.R, System.Windows.Input.Key.D, System.Windows.Input.Key.Q);

            await AppHost.StopAsync();
            AppHost.Dispose();
            base.OnExit(e);
        }
    }
}
