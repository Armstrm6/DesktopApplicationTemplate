using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.UI.Models;
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
                .ConfigureLogging(builder => builder.AddConsole().AddDebug())
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
            services.AddSingleton<IFileSearchService, DesktopApplicationTemplate.Service.Services.FileSearchService>();
            services.AddSingleton<SaveConfirmationHelper>();
            services.AddSingleton<IServiceRule, DesktopApplicationTemplate.Service.Services.ServiceRule>();
            services.AddSingleton(typeof(IServiceScreen<>), typeof(DesktopApplicationTemplate.Service.Services.ServiceScreen<>));
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
            services.AddTransient<ServiceCreateViewModelBase<MqttServiceOptions>, MqttCreateServiceViewModel>();
            services.AddTransient<MqttEditServiceView>();
            services.AddTransient<MqttEditServiceViewModel>();
            services.AddTransient<ServiceEditViewModelBase<MqttServiceOptions>, MqttEditServiceViewModel>();
            services.AddTransient<MqttAdvancedConfigView>();
            services.AddTransient<MqttAdvancedConfigViewModel>();
            services.AddTransient<TcpCreateServiceView>();
            services.AddTransient<TcpCreateServiceViewModel>();
            services.AddTransient<ServiceCreateViewModelBase<TcpServiceOptions>, TcpCreateServiceViewModel>();
            services.AddTransient<TcpEditServiceView>();
            services.AddTransient<TcpEditServiceViewModel>();
            services.AddTransient<ServiceEditViewModelBase<TcpServiceOptions>, TcpEditServiceViewModel>();
            services.AddTransient<TcpAdvancedConfigView>();
            services.AddTransient<TcpAdvancedConfigViewModel>();
            services.AddTransient<FtpServerCreateView>();
            services.AddTransient<FtpServerCreateViewModel>();
            services.AddTransient<ServiceCreateViewModelBase<DesktopApplicationTemplate.UI.Services.FtpServerOptions>, FtpServerCreateViewModel>();
            services.AddTransient<FtpServerAdvancedConfigView>();
            services.AddTransient<FtpServerAdvancedConfigViewModel>();
            services.AddTransient<FtpServerEditView>();
            services.AddTransient<FtpServerEditViewModel>();
            services.AddTransient<ServiceEditViewModelBase<DesktopApplicationTemplate.UI.Services.FtpServerOptions>, FtpServerEditViewModel>();
            services.AddTransient<HttpCreateServiceView>();
            services.AddTransient<HttpCreateServiceViewModel>();
            services.AddTransient<ServiceCreateViewModelBase<HttpServiceOptions>, HttpCreateServiceViewModel>();
            services.AddTransient<HttpEditServiceView>();
            services.AddTransient<HttpEditServiceViewModel>();
            services.AddTransient<ServiceEditViewModelBase<HttpServiceOptions>, HttpEditServiceViewModel>();
            services.AddTransient<HttpAdvancedConfigView>();
            services.AddTransient<HttpAdvancedConfigViewModel>();
            services.AddTransient<MqttEditConnectionView>();
            services.AddTransient<MqttEditConnectionViewModel>();
            services.AddTransient<MqttTagSubscriptionsView>();
            services.AddTransient<MqttTagSubscriptionsViewModel>();
            services.AddTransient<HidCreateServiceView>();
            services.AddTransient<HidCreateServiceViewModel>();
            services.AddTransient<ServiceCreateViewModelBase<HidServiceOptions>, HidCreateServiceViewModel>();
            services.AddTransient<HidEditServiceView>();
            services.AddTransient<HidEditServiceViewModel>();
            services.AddTransient<ServiceEditViewModelBase<HidServiceOptions>, HidEditServiceViewModel>();
            services.AddTransient<HidAdvancedConfigView>();
            services.AddTransient<HidAdvancedConfigViewModel>();
            services.AddTransient<HeartbeatCreateServiceView>();
            services.AddTransient<HeartbeatCreateServiceViewModel>();
            services.AddTransient<ServiceCreateViewModelBase<HeartbeatServiceOptions>, HeartbeatCreateServiceViewModel>();
            services.AddTransient<HeartbeatEditServiceView>();
            services.AddTransient<HeartbeatEditServiceViewModel>();
            services.AddTransient<ServiceEditViewModelBase<HeartbeatServiceOptions>, HeartbeatEditServiceViewModel>();
            services.AddTransient<HeartbeatAdvancedConfigView>();
            services.AddTransient<HeartbeatAdvancedConfigViewModel>();
            services.AddTransient<FileObserverCreateServiceView>();
            services.AddTransient<FileObserverCreateServiceViewModel>();
            services.AddTransient<ServiceCreateViewModelBase<FileObserverServiceOptions>, FileObserverCreateServiceViewModel>();
            services.AddTransient<FileObserverEditServiceView>();
            services.AddTransient<FileObserverEditServiceViewModel>();
            services.AddTransient<ServiceEditViewModelBase<FileObserverServiceOptions>, FileObserverEditServiceViewModel>();
            services.AddTransient<FileObserverAdvancedConfigView>();
            services.AddTransient<FileObserverAdvancedConfigViewModel>();
            services.AddTransient<CsvServiceEditorView>();
            services.AddTransient<CsvServiceEditorViewModel>();
            services.AddTransient<ServiceEditorViewModelBase<CsvServiceOptions>, CsvServiceEditorViewModel>();
            services.AddTransient<CsvAdvancedConfigView>();
            services.AddTransient<CsvAdvancedConfigViewModel>();
            services.AddTransient<ScpCreateServiceView>();
            services.AddTransient<ScpCreateServiceViewModel>();
            services.AddTransient<ServiceCreateViewModelBase<ScpServiceOptions>, ScpCreateServiceViewModel>();
            services.AddTransient<ScpEditServiceView>();
            services.AddTransient<ScpEditServiceViewModel>();
            services.AddTransient<ServiceEditViewModelBase<ScpServiceOptions>, ScpEditServiceViewModel>();
            services.AddTransient<ScpAdvancedConfigView>();
            services.AddTransient<ScpAdvancedConfigViewModel>();
            services.AddTransient<SettingsPage>();


            // Load strongly typed settings
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
            services.Configure<MqttServiceOptions>(configuration.GetSection("MqttService"));
            services.Configure<TcpServiceOptions>(configuration.GetSection("TcpService"));
            services.AddOptions<DesktopApplicationTemplate.UI.Services.FtpServerOptions>()
                .BindConfiguration("FtpServer");
            services.AddOptions<HidServiceOptions>();
            services.AddOptions<HeartbeatServiceOptions>();
            services.AddOptions<FileObserverServiceOptions>();
            services.AddOptions<CsvServiceOptions>();
            services.AddOptions<ScpServiceOptions>();
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

            var mainWindow = AppHost.Services.GetService<MainView>();
            if (mainWindow is null)
            {
                var logger = AppHost.Services.GetService<ILogger<App>>();
                logger?.LogWarning("MainView service missing; skipping window creation.");
            }
            else
            {
                mainWindow.Show();
            }

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            var logger = AppHost.Services.GetService<Microsoft.Extensions.Logging.ILogger<App>>();
            var vm = AppHost.Services.GetService<MainViewModel>();
            if (vm is null)
            {
                logger?.LogWarning("MainViewModel service missing; skipping save.");
            }
            else
            {
                vm.SaveServices();
            }

            logger?.LogInformation("Releasing keyboard state");
            Helpers.KeyboardHelper.ReleaseKeys(System.Windows.Input.Key.R, System.Windows.Input.Key.D, System.Windows.Input.Key.Q);

            await AppHost.StopAsync();
            AppHost.Dispose();
            base.OnExit(e);
        }
    }
}
