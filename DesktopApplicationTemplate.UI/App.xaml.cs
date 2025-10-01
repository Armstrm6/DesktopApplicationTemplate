using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Services.Common;
using DesktopApplicationTemplate.UI.Configuration;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Navigation;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;


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
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
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
                    .CreateLogger<App>()
                    .LogInformation(
                        "Loaded {PluginAssemblyCount} plug-in assembly(ies) from {PluginDirectory}.",
                        pluginAssemblies.Count,
                        pluginLoader.Options.RootDirectory);
            }
            catch (Exception ex)
            {
                loggerFactory
                    .CreateLogger<App>()
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
            var catalog = services.AddServiceModules(assembliesForScanning.ToArray());
            var registrations = InitializeServices(services, catalog);

            services.AddSingleton<IServiceUiRegistry>(sp => ServiceUiRegistry.Create(sp, catalog, registrations));

            services.AddSingleton<MainView>();
            services.AddSingleton<IStartupService, StartupService>();
            services.AddSingleton<IProcessRunner, ProcessRunner>();
            services.AddSingleton<INetworkConfigurationService, NetworkConfigurationService>();
            services.AddSingleton<NetworkConfigurationViewModel>();
            services.AddSingleton<IRichTextLogger, NullRichTextLogger>();
            services.AddSingleton<ILoggingService, LoggingService>();
            services.AddSingleton<IMessageRoutingService, MessageRoutingService>();
            services.AddSingleton<IFileDialogService, FileDialogService>();
            services.AddSingleton<IPluginImportService, PluginImportService>();
            services.AddCommonServices();
            services.AddSingleton<SaveConfirmationHelper>();
            services.AddSingleton<CloseConfirmationHelper>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<ServiceMessageTableViewModel>();
            services.AddSingleton<DependencyChecker>();
            services.AddFtpServer(builder => builder
                .UseDotNetFileSystem()
                .EnableAnonymousAuthentication());
            services.AddSingleton<IFtpServerService, DesktopApplicationTemplate.Service.Services.FtpServerService>();
            services.AddSingleton<SettingsViewModel>();
            services.AddTransient<SplashWindow>();
            services.AddTransient<CreateServicePage>();
            services.AddTransient<CreateServiceViewModel>();
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

        private static IReadOnlyCollection<ServiceRegistrationInfo> InitializeServices(IServiceCollection services, IServiceCatalog catalog)
        {
            var descriptorIds = new HashSet<string>(catalog.Descriptors.Select(d => d.Id), StringComparer.Ordinal);
            var registrations = new List<ServiceRegistrationInfo>();
            var registeredTypes = new HashSet<Type>();
            var registrationKeys = new HashSet<string>(StringComparer.Ordinal);
            var assembly = typeof(App).Assembly;

            foreach (var type in assembly.GetTypes())
            {
                var attributes = type.GetCustomAttributes<ServiceDescriptorRegistrationAttribute>(inherit: false);
                foreach (var attribute in attributes)
                {
                    if (!descriptorIds.Contains(attribute.DescriptorId))
                    {
                        continue;
                    }

                    if (registrationKeys.Add(CreateRegistrationKey(attribute.DescriptorId, type, attribute.Kind)))
                    {
                        registrations.Add(new ServiceRegistrationInfo(attribute.DescriptorId, type, attribute.Kind, attribute.Lifetime));
                    }

                    if (registeredTypes.Add(type))
                    {
                        RegisterType(services, type, attribute.Lifetime);
                    }
                }
            }

            ApplyConventionRegistrations(services, catalog, registrations, registeredTypes, registrationKeys, assembly);

            return registrations;
        }

        private static void RegisterType(IServiceCollection services, Type implementationType, ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton(implementationType);
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped(implementationType);
                    break;
                default:
                    services.AddTransient(implementationType);
                    break;
            }
        }

        private static void ApplyConventionRegistrations(
            IServiceCollection services,
            IServiceCatalog catalog,
            ICollection<ServiceRegistrationInfo> registrations,
            ISet<Type> registeredTypes,
            ISet<string> registrationKeys,
            Assembly assembly)
        {
            foreach (var descriptor in catalog.Descriptors)
            {
                if (!TryGetServicePrefix(descriptor, out var prefix))
                {
                    continue;
                }

                var matchingTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

                foreach (var type in matchingTypes)
                {
                    var kind = DetermineRegistrationKind(type);
                    var lifetime = DetermineLifetime(type, prefix);

                    if (registeredTypes.Add(type))
                    {
                        RegisterType(services, type, lifetime);
                    }

                    if (kind is ServiceRegistrationKind.NavigationHandler or ServiceRegistrationKind.EditHandler or ServiceRegistrationKind.ServiceFactory or ServiceRegistrationKind.ServicePage)
                    {
                        if (registrationKeys.Add(CreateRegistrationKey(descriptor.Id, type, kind.Value)))
                        {
                            registrations.Add(new ServiceRegistrationInfo(descriptor.Id, type, kind.Value, lifetime));
                        }
                    }
                }
            }
        }

        private static string CreateRegistrationKey(string descriptorId, Type implementationType, ServiceRegistrationKind kind)
        {
            var typeName = implementationType.FullName ?? implementationType.Name;
            return FormattableString.Invariant($"{descriptorId}|{typeName}|{(int)kind}");
        }

        private static bool TryGetServicePrefix(IServiceDescriptor descriptor, out string prefix)
        {
            if (descriptor.LegacyType is { } legacy)
            {
                prefix = legacy.ToString();
                return true;
            }

            prefix = descriptor.Id.Split('.').LastOrDefault() ?? descriptor.Id;
            return true;
        }

        private static ServiceRegistrationKind? DetermineRegistrationKind(Type type)
        {
            if (typeof(INavigationHandler).IsAssignableFrom(type))
            {
                return ServiceRegistrationKind.NavigationHandler;
            }

            if (typeof(IEditServiceHandler).IsAssignableFrom(type))
            {
                return ServiceRegistrationKind.EditHandler;
            }

            if (typeof(IServiceFactory).IsAssignableFrom(type))
            {
                return ServiceRegistrationKind.ServiceFactory;
            }

            if (typeof(Page).IsAssignableFrom(type))
            {
                if (type.Name.Contains("Create", StringComparison.OrdinalIgnoreCase))
                {
                    return ServiceRegistrationKind.CreateView;
                }

                if (type.Name.Contains("Edit", StringComparison.OrdinalIgnoreCase))
                {
                    return ServiceRegistrationKind.EditView;
                }

                if (type.Name.Contains("Advanced", StringComparison.OrdinalIgnoreCase))
                {
                    return ServiceRegistrationKind.AdvancedView;
                }

                return ServiceRegistrationKind.ServicePage;
            }

            if (IsViewModel(type))
            {
                if (InheritsFromGeneric(type, typeof(ServiceCreateViewModelBase<>)))
                {
                    return ServiceRegistrationKind.CreateViewModel;
                }

                if (InheritsFromGeneric(type, typeof(ServiceEditViewModelBase<>)))
                {
                    return ServiceRegistrationKind.EditViewModel;
                }

                if (InheritsFromGeneric(type, typeof(AdvancedConfigViewModelBase<>)))
                {
                    return ServiceRegistrationKind.AdvancedViewModel;
                }

                return ServiceRegistrationKind.ServicePageViewModel;
            }

            return null;
        }

        private static ServiceLifetime DetermineLifetime(Type type, string prefix)
        {
            if (typeof(IEditServiceHandler).IsAssignableFrom(type))
            {
                return ServiceLifetime.Singleton;
            }

            if (typeof(IServiceFactory).IsAssignableFrom(type) || typeof(INavigationHandler).IsAssignableFrom(type))
            {
                return ServiceLifetime.Transient;
            }

            if (typeof(Page).IsAssignableFrom(type))
            {
                if (type.Name.Contains("Create", StringComparison.OrdinalIgnoreCase) ||
                    type.Name.Contains("Edit", StringComparison.OrdinalIgnoreCase) ||
                    type.Name.Contains("Advanced", StringComparison.OrdinalIgnoreCase))
                {
                    return ServiceLifetime.Transient;
                }

                return ServiceLifetime.Singleton;
            }

            if (IsViewModel(type))
            {
                if (type.Name.Contains("Create", StringComparison.OrdinalIgnoreCase) ||
                    type.Name.Contains("Edit", StringComparison.OrdinalIgnoreCase) ||
                    type.Name.Contains("Advanced", StringComparison.OrdinalIgnoreCase))
                {
                    return ServiceLifetime.Transient;
                }

                if (type.Name.Contains("Service", StringComparison.OrdinalIgnoreCase) ||
                    type.Name.Contains("Viewer", StringComparison.OrdinalIgnoreCase) ||
                    type.Name.Equals($"{prefix}ViewModel", StringComparison.OrdinalIgnoreCase))
                {
                    return ServiceLifetime.Singleton;
                }

                return ServiceLifetime.Transient;
            }

            if (type.Namespace is { } serviceNamespace && serviceNamespace.Contains(".Services", StringComparison.Ordinal))
            {
                return ServiceLifetime.Singleton;
            }

            return ServiceLifetime.Transient;
        }

        private static bool IsViewModel(Type type) =>
            type.Name.EndsWith("ViewModel", StringComparison.Ordinal) ||
            typeof(ViewModelBase).IsAssignableFrom(type);

        private static bool InheritsFromGeneric(Type type, Type genericDefinition)
        {
            var current = type;
            while (current != null && current != typeof(object))
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == genericDefinition)
                {
                    return true;
                }

                current = current.BaseType!;
            }

            return false;
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
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
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
                MainWindow = mainWindow;
                mainWindow.Show();
                ShutdownMode = ShutdownMode.OnMainWindowClose;
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
