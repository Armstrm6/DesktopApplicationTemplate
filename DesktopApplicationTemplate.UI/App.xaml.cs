using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.EditHandlers;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Http;
using DesktopApplicationTemplate.UI.ViewModels.Tcp;
using DesktopApplicationTemplate.UI.ViewModels.Hid;
using DesktopApplicationTemplate.UI.ViewModels.Scp;
using DesktopApplicationTemplate.UI.ViewModels.Csv;
using DesktopApplicationTemplate.UI.ViewModels.FileObserver;
using DesktopApplicationTemplate.UI.ViewModels.Heartbeat;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt;
using DesktopApplicationTemplate.UI.ViewModels.Ftp;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Views.Http;
using DesktopApplicationTemplate.UI.Views.Tcp;
using DesktopApplicationTemplate.UI.Views.Hid;
using DesktopApplicationTemplate.UI.Views.Scp;
using DesktopApplicationTemplate.UI.Views.Csv;
using DesktopApplicationTemplate.UI.Views.FileObserver;
using DesktopApplicationTemplate.UI.Views.Heartbeat;
using DesktopApplicationTemplate.UI.Views.Mqtt;
using DesktopApplicationTemplate.UI.Views.Ftp;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Helpers;
using Factories = DesktopApplicationTemplate.UI.Factories;
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
using System.Windows.Controls;
using System;
using System.Windows.Threading;
using System.Collections.Generic;
using DesktopApplicationTemplate.Models;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Services.Common;


namespace DesktopApplicationTemplate.UI
{
    public partial class App : System.Windows.Application
    {
        public static IHost AppHost { get; private set; } = null!;

        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += HandleAppDomainUnhandledException;

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
            services.AddServiceModules();
            services.AddKeyedSingleton<IEditServiceHandler>(ServiceType.Mqtt, (sp, _) => new MqttEditServiceHandler(sp.GetRequiredService<MainView>(), sp.GetRequiredService<MainViewModel>(), sp, sp.GetService<ILogger<MqttEditServiceHandler>>()));
            services.AddKeyedSingleton<IEditServiceHandler>(ServiceType.Heartbeat, (sp, _) => new HeartbeatEditServiceHandler(sp.GetRequiredService<MainView>(), sp.GetRequiredService<MainViewModel>(), sp, sp.GetService<ILogger<HeartbeatEditServiceHandler>>()));
            services.AddKeyedSingleton<IEditServiceHandler>(ServiceType.Hid, (sp, _) => new HidEditServiceHandler(sp.GetRequiredService<MainView>(), sp.GetRequiredService<MainViewModel>(), sp, sp.GetService<ILogger<HidEditServiceHandler>>()));
            services.AddKeyedSingleton<IEditServiceHandler>(ServiceType.Csv, (sp, _) => new CsvEditServiceHandler(sp.GetRequiredService<MainView>(), sp.GetRequiredService<MainViewModel>(), sp, sp.GetService<ILogger<CsvEditServiceHandler>>()));
            services.AddKeyedSingleton<IEditServiceHandler>(ServiceType.FileObserver, (sp, _) => new FileObserverEditServiceHandler(sp.GetRequiredService<MainView>(), sp.GetRequiredService<MainViewModel>(), sp, sp.GetService<ILogger<FileObserverEditServiceHandler>>()));
            services.AddKeyedSingleton<IEditServiceHandler>(ServiceType.Scp, (sp, _) => new ScpEditServiceHandler(sp.GetRequiredService<MainView>(), sp.GetRequiredService<MainViewModel>(), sp, sp.GetService<ILogger<ScpEditServiceHandler>>()));
            services.AddKeyedSingleton<IEditServiceHandler>(ServiceType.Tcp, (sp, _) => new TcpEditServiceHandler(sp.GetRequiredService<MainView>(), sp.GetRequiredService<MainViewModel>(), sp, sp.GetService<ILogger<TcpEditServiceHandler>>()));
            services.AddKeyedSingleton<IEditServiceHandler>(ServiceType.Http, (sp, _) => new HttpEditServiceHandler(sp.GetRequiredService<MainView>(), sp.GetRequiredService<MainViewModel>(), sp, sp.GetService<ILogger<HttpEditServiceHandler>>()));
            services.AddKeyedSingleton<IEditServiceHandler>(ServiceType.Ftp, (sp, _) => new FtpEditServiceHandler(sp.GetRequiredService<MainView>(), sp.GetRequiredService<MainViewModel>(), sp, sp.GetService<ILogger<FtpEditServiceHandler>>()));
            services.AddSingleton<IDictionary<ServiceType, IEditServiceHandler>>(sp =>
            {
                var handlers = new Dictionary<ServiceType, IEditServiceHandler>();
                foreach (ServiceType type in Enum.GetValues<ServiceType>())
                {
                    var handler = sp.GetKeyedService<IEditServiceHandler>(type);
                    if (handler != null)
                    {
                        handlers[type] = handler;
                    }
                }
                return handlers;
            });
            services.AddSingleton<MainView>();
            services.AddSingleton<IStartupService, StartupService>();
            services.AddSingleton<IProcessRunner, ProcessRunner>();
            services.AddSingleton<INetworkConfigurationService, NetworkConfigurationService>();
            services.AddSingleton<NetworkConfigurationViewModel>();
            services.AddSingleton<IRichTextLogger, NullRichTextLogger>();
            services.AddSingleton<ILoggingService, LoggingService>();
            services.AddSingleton<IMessageRoutingService, MessageRoutingService>();
            services.AddSingleton<IFileDialogService, FileDialogService>();
            services.AddCommonServices();
            services.AddSingleton<SaveConfirmationHelper>();
            services.AddSingleton<CloseConfirmationHelper>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<ServiceMessageTableViewModel>();
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
            services.AddKeyedTransient<Page>(ServiceType.Tcp, (sp, _) => sp.GetRequiredService<TcpServiceMessagesView>());
            services.AddKeyedTransient<Page>(ServiceType.Http, (sp, _) => sp.GetRequiredService<HttpServiceView>());
            services.AddKeyedTransient<Page>(ServiceType.FileObserver, (sp, _) => sp.GetRequiredService<FileObserverView>());
            services.AddKeyedTransient<Page>(ServiceType.Hid, (sp, _) => sp.GetRequiredService<HidViews>());
            services.AddKeyedTransient<Page>(ServiceType.Heartbeat, (sp, _) => sp.GetRequiredService<HeartbeatView>());
            services.AddKeyedTransient<Page>(ServiceType.Scp, (sp, _) => sp.GetRequiredService<SCPServiceView>());
            services.AddKeyedTransient<Page>(ServiceType.Mqtt, (sp, _) => sp.GetRequiredService<MqttTagSubscriptionsView>());
            services.AddKeyedTransient<Page>(ServiceType.Ftp, (sp, _) => sp.GetRequiredService<FTPServiceView>());
            services.AddKeyedTransient<Page>(ServiceType.Csv, (sp, _) => sp.GetRequiredService<CsvServiceView>());
            services.AddSingleton<IDictionary<ServiceType, Func<Page>>>(sp => new Dictionary<ServiceType, Func<Page>>
            {
                [ServiceType.Tcp] = () => sp.GetKeyedService<Page>(ServiceType.Tcp)!,
                [ServiceType.Http] = () => sp.GetKeyedService<Page>(ServiceType.Http)!,
                [ServiceType.FileObserver] = () => sp.GetKeyedService<Page>(ServiceType.FileObserver)!,
                [ServiceType.Hid] = () => sp.GetKeyedService<Page>(ServiceType.Hid)!,
                [ServiceType.Heartbeat] = () => sp.GetKeyedService<Page>(ServiceType.Heartbeat)!,
                [ServiceType.Scp] = () => sp.GetKeyedService<Page>(ServiceType.Scp)!,
                [ServiceType.Mqtt] = () => sp.GetKeyedService<Page>(ServiceType.Mqtt)!,
                [ServiceType.Ftp] = () => sp.GetKeyedService<Page>(ServiceType.Ftp)!,
                [ServiceType.Csv] = () => sp.GetKeyedService<Page>(ServiceType.Csv)!,
            });
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
            services.AddKeyedTransient<Navigation.INavigationHandler>(ServiceType.Mqtt, (sp, _) => new Navigation.MqttNavigationHandler(sp, () => sp.GetRequiredService<MainView>()));
            services.AddKeyedTransient<Navigation.INavigationHandler>(ServiceType.Ftp, (sp, _) => new Navigation.FtpNavigationHandler(sp, () => sp.GetRequiredService<MainView>()));
            services.AddKeyedTransient<Navigation.INavigationHandler>(ServiceType.Http, (sp, _) => new Navigation.HttpNavigationHandler(sp, () => sp.GetRequiredService<MainView>()));
            services.AddKeyedTransient<Navigation.INavigationHandler>(ServiceType.Tcp, (sp, _) => new Navigation.TcpNavigationHandler(sp, () => sp.GetRequiredService<MainView>()));
            services.AddKeyedTransient<Navigation.INavigationHandler>(ServiceType.Hid, (sp, _) => new Navigation.HidNavigationHandler(sp, () => sp.GetRequiredService<MainView>()));
            services.AddKeyedTransient<Navigation.INavigationHandler>(ServiceType.Scp, (sp, _) => new Navigation.ScpNavigationHandler(sp, () => sp.GetRequiredService<MainView>()));
            services.AddKeyedTransient<Navigation.INavigationHandler>(ServiceType.Csv, (sp, _) => new Navigation.CsvNavigationHandler(sp, () => sp.GetRequiredService<MainView>()));
            services.AddKeyedTransient<Navigation.INavigationHandler>(ServiceType.FileObserver, (sp, _) => new Navigation.FileObserverNavigationHandler(sp, () => sp.GetRequiredService<MainView>()));
            services.AddKeyedTransient<Navigation.INavigationHandler>(ServiceType.Heartbeat, (sp, _) => new Navigation.HeartbeatNavigationHandler(sp, () => sp.GetRequiredService<MainView>()));
            services.AddSingleton<IDictionary<ServiceType, Navigation.INavigationHandler>>(sp =>
            {
                var handlers = new Dictionary<ServiceType, Navigation.INavigationHandler>();
                foreach (ServiceType type in Enum.GetValues<ServiceType>())
                {
                    var handler = sp.GetKeyedService<Navigation.INavigationHandler>(type);
                    if (handler != null)
                    {
                        handlers[type] = handler;
                    }
                }
                return handlers;
            });
            services.AddKeyedTransient<Factories.IServiceFactory>(ServiceType.Mqtt, (sp, _) => new Factories.MqttServiceFactory(sp, () => sp.GetRequiredService<MainView>(), sp.GetRequiredService<MainViewModel>()));
            services.AddKeyedTransient<Factories.IServiceFactory>(ServiceType.Ftp, (sp, _) => new Factories.FtpServiceFactory(sp, () => sp.GetRequiredService<MainView>()));
            services.AddKeyedTransient<Factories.IServiceFactory>(ServiceType.Http, (sp, _) => new Factories.HttpServiceFactory(() => sp.GetRequiredService<MainView>()));
            services.AddKeyedTransient<Factories.IServiceFactory>(ServiceType.Tcp, (sp, _) => new Factories.TcpServiceFactory(() => sp.GetRequiredService<MainView>()));
            services.AddKeyedTransient<Factories.IServiceFactory>(ServiceType.Hid, (sp, _) => new Factories.HidServiceFactory(() => sp.GetRequiredService<MainView>()));
            services.AddKeyedTransient<Factories.IServiceFactory>(ServiceType.Scp, (sp, _) => new Factories.ScpServiceFactory(() => sp.GetRequiredService<MainView>()));
            services.AddKeyedTransient<Factories.IServiceFactory>(ServiceType.Csv, (sp, _) => new Factories.CsvServiceFactory(() => sp.GetRequiredService<MainView>()));
            services.AddKeyedTransient<Factories.IServiceFactory>(ServiceType.FileObserver, (sp, _) => new Factories.FileObserverServiceFactory(() => sp.GetRequiredService<MainView>()));
            services.AddKeyedTransient<Factories.IServiceFactory>(ServiceType.Heartbeat, (sp, _) => new Factories.HeartbeatServiceFactory(() => sp.GetRequiredService<MainView>()));
            services.AddSingleton<IDictionary<ServiceType, Factories.IServiceFactory>>(sp =>
            {
                var factories = new Dictionary<ServiceType, Factories.IServiceFactory>();
                foreach (ServiceType type in Enum.GetValues<ServiceType>())
                {
                    var factory = sp.GetKeyedService<Factories.IServiceFactory>(type);
                    if (factory != null)
                    {
                        factories[type] = factory;
                    }
                }
                return factories;
            });


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

        internal void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var logger = AppHost.Services.GetService<ILogger<App>>();
            logger?.LogError(e.Exception, "Unhandled dispatcher exception");
            HookReleaseHelper.Release();
            e.Handled = true;
            Shutdown();
        }

        private void HandleAppDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            _ = OnAppDomainUnhandledExceptionAsync(sender, e);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD001:Use SwitchToMainThreadAsync to switch to the UI thread", Justification = "Dispatcher is sufficient for shutdown")]
        internal async Task OnAppDomainUnhandledExceptionAsync(object? sender, UnhandledExceptionEventArgs e)
        {
            var logger = AppHost.Services.GetService<ILogger<App>>();
            if (e.ExceptionObject is Exception ex)
            {
                logger?.LogError(ex, "Unhandled domain exception");
            }
            else
            {
                logger?.LogError("Unhandled domain exception");
            }

            HookReleaseHelper.Release();
            if (Current is not null)
            {
                await Current.Dispatcher.InvokeAsync(() => Current.Shutdown());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "Startup event")]
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "Application shutdown")]
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
                await vm.SaveServicesAsync().ConfigureAwait(false);
            }

            var hid = AppHost.Services.GetService<HidViewModel>();
            hid?.Dispose();

            await AppHost.StopAsync();
            AppHost.Dispose();
            base.OnExit(e);
        }
    }
}
