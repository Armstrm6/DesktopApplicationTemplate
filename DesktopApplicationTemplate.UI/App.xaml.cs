using DesktopApplicationTemplate.Core.Services.Protocols.Csv;
using DesktopApplicationTemplate.Core.Services.Protocols.FileObserver;
using DesktopApplicationTemplate.Core.Services.Protocols.Ftp;
using DesktopApplicationTemplate.Core.Services.Protocols.Heartbeat;
using DesktopApplicationTemplate.Core.Services.Protocols.Http;
using DesktopApplicationTemplate.Core.Services.Protocols.Hid;
using DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;
using DesktopApplicationTemplate.Core.Services.Protocols.Scp;
using DesktopApplicationTemplate.Core.Services.Protocols.Tcp;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Modules.BuiltIn;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Hid;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.DependencyInjection;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Helpers;
// Qualify service-layer types explicitly to avoid name clashes with UI services
using CoreFtpServerOptions = DesktopApplicationTemplate.Core.Services.Protocols.Ftp.FtpServerOptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Windows.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using DesktopApplicationTemplate.Models;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Services;
using System.Runtime.ExceptionServices;


namespace DesktopApplicationTemplate.UI
{
    public partial class App : System.Windows.Application
    {
        public static IHost AppHost { get; private set; } = null!;
        private static JoinableTaskContext UiThreadTaskContext { get; set; } = null!;
        public static JoinableTaskFactory UiThreadTaskFactory { get; private set; } = null!;
        public static IFileDialogService FileDialogService => AppHost.Services.GetRequiredService<IFileDialogService>();

        public App()
        {
            var synchronizationContext = SynchronizationContext.Current ?? new DispatcherSynchronizationContext(Dispatcher);
            UiThreadTaskContext = new JoinableTaskContext(Thread.CurrentThread, synchronizationContext);
            UiThreadTaskFactory = UiThreadTaskContext.Factory;

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

        private static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            var moduleAssemblies = new[]
            {
                typeof(MqttServiceModule).Assembly
            };

            var modules = ServiceModuleDiscovery.InstantiateModules(moduleAssemblies);
            ServiceModuleDiscovery.RegisterModules(modules, services);

            var descriptorSnapshot = ServiceModuleDiscovery.DescribeServices(modules);
            services.AddSingleton<ServiceCatalog>(_ => new ServiceCatalog(descriptorSnapshot));
            services.AddSingleton<IServiceCatalog>(sp => sp.GetRequiredService<ServiceCatalog>());

            var pluginOptions = PluginLoaderOptions.FromConfiguration(configuration);
            services.AddSingleton(pluginOptions);
            services.AddSingleton<IPluginImportService, PluginImportService>();
            services.AddSingleton<IPluginExportService, PluginExportService>();
            services.AddTransient<PluginExportViewModel>();
            services.AddTransient<PluginExportWindow>();
            services.AddSingleton<MainView>();
            services.AddSingleton<IProcessRunner, ProcessRunner>();
            services.AddSingleton<INetworkConfigurationService, NetworkConfigurationService>();
            services.AddSingleton<NetworkConfigurationViewModel>();
            services.AddSingleton<IRichTextLogger, NullRichTextLogger>();
            services.AddSingleton<ILoggingService, LoggingService>();
            services.AddSingleton<IMessageRoutingService, MessageRoutingService>();
            services.AddSingleton<IFileDialogService, FileDialogService>();
            services.AddSingleton<IStartupPreferencesService, StartupPreferencesDialogService>();
            // Register shared services and protocol facades from the Services layer.
            services.AddCommonServices();
            services.AddSingleton<SaveConfirmationHelper>();
            services.AddSingleton<CloseConfirmationHelper>();
            services.AddSingleton<IMqttClientSessionManager, MqttClientSessionManager>();
            services.AddSingleton<MainViewModel>(sp =>
            {
                var factory = UiThreadTaskFactory;
                if (factory is null)
                {
                    return ActivatorUtilities.CreateInstance<MainViewModel>(sp);
                }

                if (factory.Context.IsOnMainThread)
                {
                    return ActivatorUtilities.CreateInstance<MainViewModel>(sp);
                }

                return factory.Run(async () =>
                {
                    await factory.SwitchToMainThreadAsync();
                    return ActivatorUtilities.CreateInstance<MainViewModel>(sp);
                });
            });
            services.AddSingleton<IServiceLookup>(sp => sp.GetRequiredService<MainViewModel>());
            services.AddSingleton<SettingsViewModel>();

            services.AddCsvUi();
            services.AddFileObserverUi();
            services.AddHeartbeatUi();
            services.AddHidUi();
            services.AddHttpUi();
            services.AddMqttUi();
            services.AddScpUi();
            services.AddTcpUi();
            services.AddFtpUi();

            services.AddSingleton<IServiceUiRegistry<ServiceListModel, Page>>(sp =>
            {
                var catalog = sp.GetRequiredService<IServiceCatalog>();
                var registrations = sp.GetServices<ServiceUiRegistration<ServiceListModel, Page>>().ToArray();
                return new ServiceUiRegistry<ServiceListModel, Page>(catalog, registrations);
            });
            services.AddSingleton<IDictionary<ServiceType, IEditServiceHandler>>(sp =>
            {
                var registry = sp.GetRequiredService<IServiceUiRegistry<ServiceListModel, Page>>();
                var catalog = sp.GetRequiredService<IServiceCatalog>();
                var handlers = new ConcurrentDictionary<ServiceType, IEditServiceHandler>();

                void RefreshHandlers()
                {
                    handlers.Clear();
                    foreach (var serviceType in registry.SupportedServices)
                    {
                        if (!registry.TryGetRegistration(serviceType, out var registration))
                        {
                            continue;
                        }

                        var factory = registration.CreateEditHandler;
                        if (factory is null)
                        {
                            continue;
                        }

                        var instance = factory(sp);
                        if (instance is IEditServiceHandler handler)
                        {
                            handlers[serviceType] = handler;
                        }
                        else if (instance is not null)
                        {
                            throw new InvalidOperationException($"Edit handler factory for descriptor '{registration.DescriptorId}' returned incompatible type '{instance.GetType()}'.");
                        }
                    }
                }

                RefreshHandlers();
                catalog.DescriptorsChanged += (_, _) => RefreshHandlers();

                return handlers;
            });
            services.AddTransient<SplashWindow>();
            services.AddTransient<CreateServicePage>();
            services.AddTransient<CreateServiceViewModel>();
            services.AddTransient<SettingsPage>();


            // Load strongly typed settings
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
            services.Configure<MqttServiceOptions>(configuration.GetSection("MqttService"));
            services.Configure<TcpServiceOptions>(configuration.GetSection("TcpService"));
            services.AddOptions<CoreFtpServerOptions>()
                .BindConfiguration("FtpServer");
            services.AddOptions<HidServiceOptions>();
            services.AddOptions<HeartbeatServiceOptions>();
            services.AddOptions<FileObserverServiceOptions>();
            services.AddOptions<CsvServiceOptions>();
            services.AddOptions<ScpServiceOptions>();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var logger = AppHost?.Services?.GetService<ILogger<App>>();
            if (e.Exception is { } exception)
            {
                logger?.LogError(exception, "Unhandled dispatcher exception");
            }
            else
            {
                logger?.LogError("Unhandled dispatcher exception without an Exception instance");
            }

            e.Handled = true;

            if (UiThreadTaskFactory is null)
            {
                logger?.LogWarning("Joinable task factory unavailable during dispatcher exception; shutting down synchronously.");
                KeyboardSimulator.Reset();
                Current?.Shutdown();
                return;
            }

            UiThreadTaskFactory.Run(async () =>
            {
                KeyboardSimulator.Reset();
                await Task.Yield();
                Current?.Shutdown();
            });
        }

        private void HandleAppDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            _ = OnAppDomainUnhandledExceptionAsync(sender, e);
        }

        internal static async Task OnAppDomainUnhandledExceptionAsync(object? sender, UnhandledExceptionEventArgs e)
        {
            var logger = AppHost?.Services?.GetService<ILogger<App>>();
            if (e.ExceptionObject is Exception ex)
            {
                logger?.LogError(ex, "Unhandled domain exception");
            }
            else
            {
                logger?.LogError("Unhandled domain exception");
            }

            KeyboardSimulator.Reset();
            if (UiThreadTaskFactory is null)
            {
                logger?.LogWarning("Joinable task factory unavailable during domain exception; scheduling shutdown on dispatcher context");
                if (Current?.Dispatcher is { } dispatcher)
                {
                    var dispatcherContext = new DispatcherSynchronizationContext(dispatcher);
                    dispatcherContext.Post(_ => Current?.Shutdown(), null);
                }
                else
                {
                    Current?.Shutdown();
                }

                return;
            }

            await UiThreadTaskFactory.SwitchToMainThreadAsync();
            Current?.Shutdown();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ExceptionDispatchInfo? capturedException = null;

            try
            {
                UiThreadTaskFactory.Run(OnStartupAsync);
            }
            catch (Exception ex)
            {
                capturedException = ExceptionDispatchInfo.Capture(ex);
            }

            base.OnStartup(e);

            capturedException?.Throw();
        }

        private static async Task OnStartupAsync()
        {
            var application = Current ?? throw new InvalidOperationException("Application.Current is unavailable.");
            application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            await AppHost.StartAsync();

            var settings = AppHost.Services.GetRequiredService<SettingsViewModel>();
            await settings.LoadAsync().ConfigureAwait(false);
            await UiThreadTaskFactory.SwitchToMainThreadAsync();
            Services.ThemeManager.ApplyTheme(settings.DarkTheme);

            var mainViewModel = AppHost.Services.GetRequiredService<MainViewModel>();
            await mainViewModel.LoadServicesAsync().ConfigureAwait(true);

            SplashWindow? splash = null;
            if (settings.FirstRun)
            {
                splash = AppHost.Services.GetRequiredService<SplashWindow>();
                splash.Show();
            }

            splash?.Close();
            if (splash != null)
            {
                settings.FirstRun = false;
                await settings.SaveAsync().ConfigureAwait(false);
                await UiThreadTaskFactory.SwitchToMainThreadAsync();
            }

            await UiThreadTaskFactory.SwitchToMainThreadAsync();

            var mainWindow = AppHost.Services.GetService<MainView>();
            if (mainWindow is null)
            {
                var logger = AppHost.Services.GetService<ILogger<App>>();
                logger?.LogWarning("MainView service missing; skipping window creation.");
                return;
            }

            application.MainWindow = mainWindow;
            mainWindow.Show();
            application.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ExceptionDispatchInfo? capturedException = null;

            try
            {
                UiThreadTaskFactory.Run(OnExitAsync);
            }
            catch (Exception ex)
            {
                capturedException = ExceptionDispatchInfo.Capture(ex);
            }

            base.OnExit(e);

            capturedException?.Throw();
        }

        private static async Task OnExitAsync()
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

            await AppHost.StopAsync().ConfigureAwait(false);
            AppHost.Dispose();
        }
    }
}
