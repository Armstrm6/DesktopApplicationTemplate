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
using DesktopApplicationTemplate.UI.EditHandlers;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Modules;
using DesktopApplicationTemplate.Core.Modules.BuiltIn;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Http;
using DesktopApplicationTemplate.UI.ViewModels.Http.Create;
using DesktopApplicationTemplate.UI.ViewModels.Http.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Http.Advanced;
using DesktopApplicationTemplate.UI.ViewModels.Tcp;
using DesktopApplicationTemplate.UI.ViewModels.Tcp.Create;
using DesktopApplicationTemplate.UI.ViewModels.Tcp.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Hid;
using DesktopApplicationTemplate.UI.ViewModels.Hid.Create;
using DesktopApplicationTemplate.UI.ViewModels.Hid.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Hid.Advanced;
using DesktopApplicationTemplate.UI.ViewModels.Scp;
using DesktopApplicationTemplate.UI.ViewModels.Scp.Create;
using DesktopApplicationTemplate.UI.ViewModels.Scp.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Scp.Advanced;
using DesktopApplicationTemplate.UI.ViewModels.Csv;
using DesktopApplicationTemplate.UI.ViewModels.Csv.Edit;
using DesktopApplicationTemplate.UI.ViewModels.FileObserver;
using DesktopApplicationTemplate.UI.ViewModels.FileObserver.Create;
using DesktopApplicationTemplate.UI.ViewModels.FileObserver.Edit;
using DesktopApplicationTemplate.UI.ViewModels.FileObserver.Advanced;
using DesktopApplicationTemplate.UI.ViewModels.Heartbeat;
using DesktopApplicationTemplate.UI.ViewModels.Heartbeat.Create;
using DesktopApplicationTemplate.UI.ViewModels.Heartbeat.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Heartbeat.Advanced;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt.Create;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Ftp;
using DesktopApplicationTemplate.UI.ViewModels.Ftp.Create;
using DesktopApplicationTemplate.UI.ViewModels.Ftp.Edit;
using DesktopApplicationTemplate.UI.ViewModels.Ftp.Advanced;
using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Views.Http;
using DesktopApplicationTemplate.UI.Views.Http.Create;
using DesktopApplicationTemplate.UI.Views.Http.Edit;
using DesktopApplicationTemplate.UI.Views.Http.Advanced;
using DesktopApplicationTemplate.UI.Views.Tcp;
using DesktopApplicationTemplate.UI.Views.Tcp.Create;
using DesktopApplicationTemplate.UI.Views.Tcp.Edit;
using DesktopApplicationTemplate.UI.Views.Hid;
using DesktopApplicationTemplate.UI.Views.Hid.Create;
using DesktopApplicationTemplate.UI.Views.Hid.Edit;
using DesktopApplicationTemplate.UI.Views.Hid.Advanced;
using DesktopApplicationTemplate.UI.Views.Scp;
using DesktopApplicationTemplate.UI.Views.Scp.Create;
using DesktopApplicationTemplate.UI.Views.Scp.Edit;
using DesktopApplicationTemplate.UI.Views.Scp.Advanced;
using DesktopApplicationTemplate.UI.Views.Csv;
using DesktopApplicationTemplate.UI.Views.Csv.Edit;
using DesktopApplicationTemplate.UI.Views.FileObserver;
using DesktopApplicationTemplate.UI.Views.FileObserver.Create;
using DesktopApplicationTemplate.UI.Views.FileObserver.Edit;
using DesktopApplicationTemplate.UI.Views.FileObserver.Advanced;
using DesktopApplicationTemplate.UI.Views.Heartbeat;
using DesktopApplicationTemplate.UI.Views.Heartbeat.Create;
using DesktopApplicationTemplate.UI.Views.Heartbeat.Edit;
using DesktopApplicationTemplate.UI.Views.Heartbeat.Advanced;
using DesktopApplicationTemplate.UI.Views.Mqtt;
using DesktopApplicationTemplate.UI.Views.Mqtt.Create;
using DesktopApplicationTemplate.UI.Views.Mqtt.Edit;
using DesktopApplicationTemplate.UI.Views.Ftp;
using DesktopApplicationTemplate.UI.Views.Ftp.Create;
using DesktopApplicationTemplate.UI.Views.Ftp.Edit;
using DesktopApplicationTemplate.UI.Views.Ftp.Advanced;
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
using MQTTnet;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Threading;
using DesktopApplicationTemplate.Models;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Services;


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

            services.AddKeyedSingleton<IEditServiceHandler>(ServiceType.Mqtt, (sp, _) => new MqttEditServiceHandler(() => sp.GetRequiredService<MainView>(), () => sp.GetRequiredService<MainViewModel>(), sp, sp.GetService<ILogger<MqttEditServiceHandler>>()));
            services.AddKeyedSingleton<IEditServiceHandler>(ServiceType.Heartbeat, (sp, _) => new HeartbeatEditServiceHandler(() => sp.GetRequiredService<MainView>(), () => sp.GetRequiredService<MainViewModel>(), sp, sp.GetService<ILogger<HeartbeatEditServiceHandler>>()));
            services.AddKeyedSingleton<IEditServiceHandler>(ServiceType.Hid, (sp, _) => new HidEditServiceHandler(() => sp.GetRequiredService<MainView>(), () => sp.GetRequiredService<MainViewModel>(), sp, sp.GetService<ILogger<HidEditServiceHandler>>()));
            services.AddKeyedSingleton<IEditServiceHandler>(ServiceType.Csv, (sp, _) => new CsvEditServiceHandler(() => sp.GetRequiredService<MainView>(), () => sp.GetRequiredService<MainViewModel>(), sp, sp.GetService<ILogger<CsvEditServiceHandler>>()));
            services.AddKeyedSingleton<IEditServiceHandler>(ServiceType.FileObserver, (sp, _) => new FileObserverEditServiceHandler(() => sp.GetRequiredService<MainView>(), () => sp.GetRequiredService<MainViewModel>(), sp, sp.GetService<ILogger<FileObserverEditServiceHandler>>()));
            services.AddKeyedSingleton<IEditServiceHandler>(ServiceType.Scp, (sp, _) => new ScpEditServiceHandler(() => sp.GetRequiredService<MainView>(), () => sp.GetRequiredService<MainViewModel>(), sp, sp.GetService<ILogger<ScpEditServiceHandler>>()));
            services.AddKeyedSingleton<IEditServiceHandler>(ServiceType.Tcp, (sp, _) => new TcpEditServiceHandler(() => sp.GetRequiredService<MainView>(), () => sp.GetRequiredService<MainViewModel>(), sp, sp.GetService<ILogger<TcpEditServiceHandler>>()));
            services.AddKeyedSingleton<IEditServiceHandler>(ServiceType.Http, (sp, _) => new HttpEditServiceHandler(() => sp.GetRequiredService<MainView>(), () => sp.GetRequiredService<MainViewModel>(), sp, sp.GetService<ILogger<HttpEditServiceHandler>>()));
            services.AddKeyedSingleton<IEditServiceHandler>(ServiceType.Ftp, (sp, _) => new FtpEditServiceHandler(() => sp.GetRequiredService<MainView>(), () => sp.GetRequiredService<MainViewModel>(), sp, sp.GetService<ILogger<FtpEditServiceHandler>>()));
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
            services.AddSingleton<MainViewModel>();
            services.AddTransient<ServiceMessageTableViewModel>();
            services.AddTransient<Func<ServiceMessageTableViewModel>>(sp => () => sp.GetRequiredService<ServiceMessageTableViewModel>());
            services.AddTransient<TcpServiceMessagesView>();
            services.AddTransient<TcpServiceMessagesViewModel>();
            services.AddTransient<HttpServiceView>();
            services.AddTransient<HttpServiceViewModel>();
            services.AddTransient<FileObserverView>();
            services.AddTransient<FileObserverViewModel>();
            services.AddTransient<HeartbeatView>();
            services.AddTransient<HeartbeatViewModel>();
            services.AddTransient<SCPServiceView>();
            services.AddTransient<ScpServiceViewModel>();
            services.AddTransient<HidViewModel>();
            services.AddTransient<HidViews>();
            services.AddTransient<FTPServiceView>();
            services.AddTransient<FtpServiceViewModel>();
            services.AddTransient<CsvViewerViewModel>();
            services.AddSingleton<ICsvOutput, FileCsvOutput>();
            services.AddTransient<CsvServiceAdapter>();
            services.AddTransient<CsvServiceView>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<IServiceUiRegistry<ServiceListModel, Page>>(sp =>
            {
                var catalog = sp.GetRequiredService<IServiceCatalog>();
                return new ServiceUiRegistry<ServiceListModel, Page>(
                    catalog,
                    [
                        BuildCsvRegistration(catalog),
                        BuildFileObserverRegistration(catalog),
                        BuildHeartbeatRegistration(catalog),
                        BuildHidRegistration(catalog),
                        BuildHttpRegistration(catalog),
                        BuildMqttRegistration(catalog),
                        BuildScpRegistration(catalog),
                        BuildTcpRegistration(catalog),
                        BuildFtpRegistration(catalog),
                    ]);
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
            services.AddTransient<TcpCreateServiceView>();
            services.AddTransient<TcpCreateServiceViewModel>();
            services.AddTransient<ServiceCreateViewModelBase<TcpServiceOptions>, TcpCreateServiceViewModel>();
            services.AddTransient<TcpEditServiceView>();
            services.AddTransient<TcpEditServiceViewModel>();
            services.AddTransient<ServiceEditViewModelBase<TcpServiceOptions>, TcpEditServiceViewModel>();
            services.AddTransient<FtpServerCreateView>();
            services.AddTransient<FtpServerCreateViewModel>();
            services.AddTransient<ServiceCreateViewModelBase<CoreFtpServerOptions>, FtpServerCreateViewModel>();
            services.AddTransient<FtpServerAdvancedConfigView>();
            services.AddTransient<FtpServerAdvancedConfigViewModel>();
            services.AddTransient<FtpServerEditView>();
            services.AddTransient<FtpServerEditViewModel>();
            services.AddTransient<ServiceEditViewModelBase<CoreFtpServerOptions>, FtpServerEditViewModel>();
            services.AddTransient<HttpCreateServiceView>();
            services.AddTransient<HttpCreateServiceViewModel>();
            services.AddTransient<ServiceCreateViewModelBase<HttpServiceOptions>, HttpCreateServiceViewModel>();
            services.AddTransient<HttpEditServiceView>();
            services.AddTransient<HttpEditServiceViewModel>();
            services.AddTransient<ServiceEditViewModelBase<HttpServiceOptions>, HttpEditServiceViewModel>();
            services.AddTransient<HttpAdvancedConfigView>();
            services.AddTransient<HttpAdvancedConfigViewModel>();
            services.AddTransient<MqttEditConnectionView>();
            services.AddTransient<MqttTagSubscriptionsView>();
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
            services.AddOptions<CoreFtpServerOptions>()
                .BindConfiguration("FtpServer");
            services.AddOptions<HidServiceOptions>();
            services.AddOptions<HeartbeatServiceOptions>();
            services.AddOptions<FileObserverServiceOptions>();
            services.AddOptions<CsvServiceOptions>();
            services.AddOptions<ScpServiceOptions>();
        }

        private static ServiceUiRegistration<ServiceListModel, Page> BuildMqttRegistration(IServiceCatalog catalog)
        {
            var descriptor = GetDescriptorOrThrow(catalog, ServiceDescriptorIds.Mqtt);

            return new ServiceUiRegistration<ServiceListModel, Page>(
                descriptor.Id,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<MqttServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var newService = new ServiceListModel
                    {
                        DescriptorId = descriptor.Id,
                        DisplayName = ctx.Name,
                        Type = ServiceType.Mqtt,
                        IsActive = false
                    };

                    newService.SetOptions(ctx.Options ?? new MqttServiceOptions());
                    mainView.GetOrCreateServicePage(newService);

                    return newService;
                },
                provider => provider.GetRequiredService<MqttTagSubscriptionsView>(),
                (provider, defaultName) =>
                {
                    var vm = provider.GetRequiredService<MqttCreateServiceViewModel>();
                    vm.ServiceName = defaultName;
                    var mainView = provider.GetRequiredService<MainView>();
                    vm.ServiceSaved += (name, options) =>
                    {
                        QueueServiceAddition(mainView, ServiceType.Mqtt, name, (MqttServiceOptions)options);
                    };
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    var view = ActivatorUtilities.CreateInstance<MqttCreateServiceView>(provider, vm);
                    return view;
                },
                ApplyPresentation: static (service, metadata) => service.ApplyPresentation(metadata));
        }

        private static ServiceUiRegistration<ServiceListModel, Page> BuildFtpRegistration(IServiceCatalog catalog)
        {
            var descriptor = GetDescriptorOrThrow(catalog, ServiceDescriptorIds.Ftp);

            return new ServiceUiRegistration<ServiceListModel, Page>(
                descriptor.Id,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<CoreFtpServerOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var ftpOptions = ctx.Options ?? new CoreFtpServerOptions();
                    var svc = new ServiceListModel
                    {
                        DescriptorId = descriptor.Id,
                        DisplayName = ctx.Name,
                        Type = ServiceType.Ftp,
                        IsActive = false,
                    };

                    svc.SetOptions(ftpOptions);

                    mainView.GetOrCreateServicePage(svc);

                    return svc;
                },
                provider => provider.GetRequiredService<FTPServiceView>(),
                (provider, defaultName) =>
                {
                    var vm = provider.GetRequiredService<FtpServerCreateViewModel>();
                    vm.ServiceName = defaultName;
                    var mainView = provider.GetRequiredService<MainView>();
                    vm.ServiceSaved += (name, options) =>
                    {
                        QueueServiceAddition(mainView, ServiceType.Ftp, name, (CoreFtpServerOptions)options);
                    };
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    var view = ActivatorUtilities.CreateInstance<FtpServerCreateView>(provider, vm);
                    vm.AdvancedConfigRequested += opts =>
                    {
                        var advVm = ActivatorUtilities.CreateInstance<FtpServerAdvancedConfigViewModel>(provider, opts);
                        var advView = provider.GetRequiredService<FtpServerAdvancedConfigView>();
                        advView.Initialize(advVm);
                        advVm.Saved += _ => mainView.ShowPage(view);
                        advVm.BackRequested += () => mainView.ShowPage(view);
                        mainView.ShowPage(advView);
                    };
                    return view;
                },
                ApplyPresentation: static (service, metadata) => service.ApplyPresentation(metadata));
        }

        private static ServiceUiRegistration<ServiceListModel, Page> BuildHttpRegistration(IServiceCatalog catalog)
        {
            var descriptor = GetDescriptorOrThrow(catalog, ServiceDescriptorIds.Http);

            return new ServiceUiRegistration<ServiceListModel, Page>(
                descriptor.Id,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<HttpServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var svc = new ServiceListModel
                    {
                        DescriptorId = descriptor.Id,
                        DisplayName = ctx.Name,
                        Type = ServiceType.Http,
                        IsActive = false,
                    };

                    svc.SetOptions(ctx.Options);

                    mainView.GetOrCreateServicePage(svc);
                    return svc;
                },
                provider => provider.GetRequiredService<HttpServiceView>(),
                (provider, defaultName) =>
                {
                    var vm = provider.GetRequiredService<HttpCreateServiceViewModel>();
                    vm.ServiceName = defaultName;
                    var mainView = provider.GetRequiredService<MainView>();
                    vm.ServiceSaved += (name, options) =>
                    {
                        QueueServiceAddition(mainView, ServiceType.Http, name, (HttpServiceOptions)options);
                    };
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    var view = ActivatorUtilities.CreateInstance<HttpCreateServiceView>(provider, vm);
                    vm.AdvancedConfigRequested += opts =>
                    {
                        var advVm = ActivatorUtilities.CreateInstance<HttpAdvancedConfigViewModel>(provider, opts);
                        var advView = provider.GetRequiredService<HttpAdvancedConfigView>();
                        advView.Initialize(advVm);
                        advVm.Saved += _ => mainView.ShowPage(view);
                        advVm.BackRequested += () => mainView.ShowPage(view);
                        mainView.ShowPage(advView);
                    };
                    return view;
                },
                ApplyPresentation: static (service, metadata) => service.ApplyPresentation(metadata));
        }

        private static ServiceUiRegistration<ServiceListModel, Page> BuildTcpRegistration(IServiceCatalog catalog)
        {
            var descriptor = GetDescriptorOrThrow(catalog, ServiceDescriptorIds.Tcp);

            return new ServiceUiRegistration<ServiceListModel, Page>(
                descriptor.Id,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<TcpServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var svc = new ServiceListModel
                    {
                        DescriptorId = descriptor.Id,
                        DisplayName = ctx.Name,
                        Type = ServiceType.Tcp,
                        IsActive = false,
                    };

                    svc.SetOptions(ctx.Options);

                    mainView.GetOrCreateServicePage(svc);
                    return svc;
                },
                provider => provider.GetRequiredService<TcpServiceMessagesView>(),
                (provider, defaultName) =>
                {
                    var vm = provider.GetRequiredService<TcpCreateServiceViewModel>();
                    vm.ServiceName = defaultName;
                    var mainView = provider.GetRequiredService<MainView>();
                    vm.ServiceSaved += (name, options) =>
                    {
                        QueueServiceAddition(mainView, ServiceType.Tcp, name, (TcpServiceOptions)options);
                    };
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    return ActivatorUtilities.CreateInstance<TcpCreateServiceView>(provider, vm);
                },
                ApplyPresentation: static (service, metadata) => service.ApplyPresentation(metadata));
        }

        private static ServiceUiRegistration<ServiceListModel, Page> BuildHidRegistration(IServiceCatalog catalog)
        {
            var descriptor = GetDescriptorOrThrow(catalog, ServiceDescriptorIds.Hid);

            return new ServiceUiRegistration<ServiceListModel, Page>(
                descriptor.Id,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<HidServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var svc = new ServiceListModel
                    {
                        DescriptorId = descriptor.Id,
                        DisplayName = ctx.Name,
                        Type = ServiceType.Hid,
                        IsActive = false,
                    };

                    svc.SetOptions(ctx.Options);

                    mainView.GetOrCreateServicePage(svc);
                    return svc;
                },
                provider => provider.GetRequiredService<HidViews>(),
                (provider, defaultName) =>
                {
                    var vm = provider.GetRequiredService<HidCreateServiceViewModel>();
                    vm.ServiceName = defaultName;
                    var mainView = provider.GetRequiredService<MainView>();
                    vm.ServiceSaved += (name, options) =>
                    {
                        QueueServiceAddition(mainView, ServiceType.Hid, name, (HidServiceOptions)options);
                    };
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    var view = ActivatorUtilities.CreateInstance<HidCreateServiceView>(provider, vm);
                    vm.AdvancedConfigRequested += opts =>
                    {
                        var advVm = ActivatorUtilities.CreateInstance<HidAdvancedConfigViewModel>(provider, opts);
                        var advView = provider.GetRequiredService<HidAdvancedConfigView>();
                        advView.Initialize(advVm);
                        advVm.Saved += _ => mainView.ShowPage(view);
                        advVm.BackRequested += () => mainView.ShowPage(view);
                        mainView.ShowPage(advView);
                    };
                    return view;
                },
                ApplyPresentation: static (service, metadata) => service.ApplyPresentation(metadata));
        }

        private static ServiceUiRegistration<ServiceListModel, Page> BuildScpRegistration(IServiceCatalog catalog)
        {
            var descriptor = GetDescriptorOrThrow(catalog, ServiceDescriptorIds.Scp);

            return new ServiceUiRegistration<ServiceListModel, Page>(
                descriptor.Id,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<ScpServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var svc = new ServiceListModel
                    {
                        DescriptorId = descriptor.Id,
                        DisplayName = ctx.Name,
                        Type = ServiceType.Scp,
                        IsActive = false,
                    };

                    svc.SetOptions(ctx.Options);

                    mainView.GetOrCreateServicePage(svc);
                    return svc;
                },
                provider => provider.GetRequiredService<SCPServiceView>(),
                (provider, defaultName) =>
                {
                    var vm = provider.GetRequiredService<ScpCreateServiceViewModel>();
                    vm.ServiceName = defaultName;
                    var mainView = provider.GetRequiredService<MainView>();
                    vm.ServiceSaved += (name, options) =>
                    {
                        QueueServiceAddition(mainView, ServiceType.Scp, name, (ScpServiceOptions)options);
                    };
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    var view = ActivatorUtilities.CreateInstance<ScpCreateServiceView>(provider, vm);
                    vm.AdvancedConfigRequested += opts =>
                    {
                        var advVm = ActivatorUtilities.CreateInstance<ScpAdvancedConfigViewModel>(provider, opts);
                        var advView = provider.GetRequiredService<ScpAdvancedConfigView>();
                        advView.Initialize(advVm);
                        advVm.Saved += _ => mainView.ShowPage(view);
                        advVm.BackRequested += () => mainView.ShowPage(view);
                        mainView.ShowPage(advView);
                    };
                    return view;
                },
                ApplyPresentation: static (service, metadata) => service.ApplyPresentation(metadata));
        }

        private static ServiceUiRegistration<ServiceListModel, Page> BuildFileObserverRegistration(IServiceCatalog catalog)
        {
            var descriptor = GetDescriptorOrThrow(catalog, ServiceDescriptorIds.FileObserver);

            return new ServiceUiRegistration<ServiceListModel, Page>(
                descriptor.Id,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<FileObserverServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var svc = new ServiceListModel
                    {
                        DescriptorId = descriptor.Id,
                        DisplayName = ctx.Name,
                        Type = ServiceType.FileObserver,
                        IsActive = false,
                    };

                    svc.SetOptions(ctx.Options);

                    mainView.GetOrCreateServicePage(svc);
                    return svc;
                },
                provider => provider.GetRequiredService<FileObserverView>(),
                (provider, defaultName) =>
                {
                    var vm = provider.GetRequiredService<FileObserverCreateServiceViewModel>();
                    vm.ServiceName = defaultName;
                    var mainView = provider.GetRequiredService<MainView>();
                    vm.ServiceSaved += (name, options) =>
                    {
                        QueueServiceAddition(mainView, ServiceType.FileObserver, name, (FileObserverServiceOptions)options);
                    };
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    var view = ActivatorUtilities.CreateInstance<FileObserverCreateServiceView>(provider, vm);
                    vm.AdvancedConfigRequested += opts =>
                    {
                        var advVm = ActivatorUtilities.CreateInstance<FileObserverAdvancedConfigViewModel>(provider, opts);
                        var advView = provider.GetRequiredService<FileObserverAdvancedConfigView>();
                        advView.Initialize(advVm);
                        advVm.Saved += _ => mainView.ShowPage(view);
                        advVm.BackRequested += () => mainView.ShowPage(view);
                        mainView.ShowPage(advView);
                    };
                    return view;
                },
                ApplyPresentation: static (service, metadata) => service.ApplyPresentation(metadata));
        }

        private static ServiceUiRegistration<ServiceListModel, Page> BuildCsvRegistration(IServiceCatalog catalog)
        {
            var descriptor = GetDescriptorOrThrow(catalog, ServiceDescriptorIds.Csv);

            return new ServiceUiRegistration<ServiceListModel, Page>(
                descriptor.Id,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<CsvServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var svc = new ServiceListModel
                    {
                        DescriptorId = descriptor.Id,
                        DisplayName = ctx.Name,
                        Type = ServiceType.Csv,
                        IsActive = false,
                    };

                    svc.SetOptions(ctx.Options);

                    mainView.GetOrCreateServicePage(svc);
                    return svc;
                },
                provider => provider.GetRequiredService<CsvServiceView>(),
                (provider, defaultName) =>
                {
                    var vm = provider.GetRequiredService<CsvServiceEditorViewModel>();
                    vm.ServiceName = defaultName;
                    var mainView = provider.GetRequiredService<MainView>();
                    vm.ServiceSaved += (name, options) =>
                    {
                        QueueServiceAddition(mainView, ServiceType.Csv, name, (CsvServiceOptions)options);
                    };
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    var view = provider.GetRequiredService<CsvServiceEditorView>();
                    view.Initialize(vm);
                    return view;
                },
                ApplyPresentation: static (service, metadata) => service.ApplyPresentation(metadata));
        }

        private static ServiceUiRegistration<ServiceListModel, Page> BuildHeartbeatRegistration(IServiceCatalog catalog)
        {
            var descriptor = GetDescriptorOrThrow(catalog, ServiceDescriptorIds.Heartbeat);

            return new ServiceUiRegistration<ServiceListModel, Page>(
                descriptor.Id,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<HeartbeatServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var svc = new ServiceListModel
                    {
                        DescriptorId = descriptor.Id,
                        DisplayName = ctx.Name,
                        Type = ServiceType.Heartbeat,
                        IsActive = false,
                    };

                    svc.SetOptions(ctx.Options);

                    mainView.GetOrCreateServicePage(svc);
                    return svc;
                },
                provider => provider.GetRequiredService<HeartbeatView>(),
                (provider, defaultName) =>
                {
                    var vm = provider.GetRequiredService<HeartbeatCreateServiceViewModel>();
                    vm.ServiceName = defaultName;
                    var mainView = provider.GetRequiredService<MainView>();
                    vm.ServiceSaved += (name, options) =>
                    {
                        QueueServiceAddition(mainView, ServiceType.Heartbeat, name, (HeartbeatServiceOptions)options);
                    };
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    var view = ActivatorUtilities.CreateInstance<HeartbeatCreateServiceView>(provider, vm);
                    vm.AdvancedConfigRequested += opts =>
                    {
                        var advVm = ActivatorUtilities.CreateInstance<HeartbeatAdvancedConfigViewModel>(provider, opts);
                        var advView = provider.GetRequiredService<HeartbeatAdvancedConfigView>();
                        advView.Initialize(advVm);
                        advVm.Saved += _ => mainView.ShowPage(view);
                        advVm.BackRequested += () => mainView.ShowPage(view);
                        mainView.ShowPage(advView);
                    };
                    return view;
                },
                ApplyPresentation: static (service, metadata) => service.ApplyPresentation(metadata));
        }

        private static IServiceDescriptor GetDescriptorOrThrow(IServiceCatalog catalog, string descriptorId)
        {
            if (catalog is null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (descriptorId is null)
            {
                throw new ArgumentNullException(nameof(descriptorId));
            }

            if (catalog.TryGetById(descriptorId, out var descriptor))
            {
                return descriptor;
            }

            throw new InvalidOperationException($"Descriptor '{descriptorId}' is not registered.");
        }

        private static void QueueServiceAddition<TOptions>(MainView mainView, ServiceType serviceType, string serviceName, TOptions options)
        {
            async Task ExecuteAsync()
            {
                await mainView.TryAddServiceAsync(serviceType, new ServiceFactoryOptions<TOptions>(serviceName, options));
            }

            if (UiThreadTaskFactory is null)
            {
                ObserveTaskFailure(ExecuteAsync(), serviceType, serviceName);
                return;
            }

            var joinableTask = UiThreadTaskFactory.RunAsync(async () =>
            {
                await UiThreadTaskFactory.SwitchToMainThreadAsync();
                await ExecuteAsync();
            });

            ObserveTaskFailure(joinableTask.Task, serviceType, serviceName);
        }

        private static void ObserveTaskFailure(Task task, ServiceType serviceType, string serviceName)
        {
            _ = task.ContinueWith(t =>
            {
                if (t.IsFaulted && t.Exception is { } exception)
                {
                    var logger = AppHost.Services.GetService<ILogger<App>>();
                    logger?.LogError(exception, "Failed to add {ServiceType} service {ServiceName}", serviceType, serviceName);
                }
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        internal void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var logger = AppHost.Services.GetService<ILogger<App>>();
            logger?.LogError(e.Exception, "Unhandled dispatcher exception");
            KeyboardSimulator.Reset();
            e.Handled = true;
            Shutdown();
        }

        private void HandleAppDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            _ = OnAppDomainUnhandledExceptionAsync(sender, e);
        }

        internal static async Task OnAppDomainUnhandledExceptionAsync(object? sender, UnhandledExceptionEventArgs e)
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
            UiThreadTaskFactory.Run(() => OnStartupAsync());
            base.OnStartup(e);
        }

        private static async Task OnStartupAsync()
        {
            var application = Current ?? throw new InvalidOperationException("Application.Current is unavailable.");
            application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
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
                return;
            }

            application.MainWindow = mainWindow;
            mainWindow.Show();
            application.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            UiThreadTaskFactory.Run(OnExitAsync);
            base.OnExit(e);
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
