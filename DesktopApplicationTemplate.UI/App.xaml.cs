using DesktopApplicationTemplate.Core.Services.Protocols.Csv;
using DesktopApplicationTemplate.Core.Services.Protocols.FileObserver;
using DesktopApplicationTemplate.Core.Services.Protocols.Ftp;
using DesktopApplicationTemplate.Core.Services.Protocols.Heartbeat;
using DesktopApplicationTemplate.Core.Services.Protocols.Http;
using DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;
using DesktopApplicationTemplate.Core.Services.Protocols.Tcp;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.EditHandlers;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Modules;
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
using DesktopApplicationTemplate.UI.ViewModels.Csv.Advanced;
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
using DesktopApplicationTemplate.UI.ViewModels.Mqtt.Advanced;
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
using DesktopApplicationTemplate.UI.Views.Csv.Advanced;
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
using DesktopApplicationTemplate.UI.Views.Mqtt.Advanced;
using DesktopApplicationTemplate.UI.Views.Ftp;
using DesktopApplicationTemplate.UI.Views.Ftp.Create;
using DesktopApplicationTemplate.UI.Views.Ftp.Edit;
using DesktopApplicationTemplate.UI.Views.Ftp.Advanced;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Helpers;
// Qualify service-layer types explicitly to avoid name clashes with UI services
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
using DesktopApplicationTemplate.Services;


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
            services.AddCommonServices();
            services.AddSingleton<SaveConfirmationHelper>();
            services.AddSingleton<CloseConfirmationHelper>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<ServiceMessageTableViewModel>();
            services.AddSingleton<TcpServiceMessagesView>();
            services.AddTransient<TcpServiceMessagesViewModel>();
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
            services.AddSingleton<IFtpServerService, DesktopApplicationTemplate.Services.FtpServerService>();
            services.AddSingleton<CsvViewerViewModel>();
            services.AddSingleton<CsvService>();
            services.AddSingleton<CsvServiceView>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<IServiceUiRegistry<ServiceListModel, Page>>(_ =>
                new ServiceUiRegistry<ServiceListModel, Page>(new[]
                {
                    BuildCsvRegistration(),
                    BuildFileObserverRegistration(),
                    BuildHeartbeatRegistration(),
                    BuildHidRegistration(),
                    BuildHttpRegistration(),
                    BuildMqttRegistration(),
                    BuildScpRegistration(),
                    BuildTcpRegistration(),
                    BuildFtpRegistration(),
                }));
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
            services.AddTransient<ServiceCreateViewModelBase<FtpServerOptions>, FtpServerCreateViewModel>();
            services.AddTransient<FtpServerAdvancedConfigView>();
            services.AddTransient<FtpServerAdvancedConfigViewModel>();
            services.AddTransient<FtpServerEditView>();
            services.AddTransient<FtpServerEditViewModel>();
            services.AddTransient<ServiceEditViewModelBase<FtpServerOptions>, FtpServerEditViewModel>();
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
            services.AddOptions<FtpServerOptions>()
                .BindConfiguration("FtpServer");
            services.AddOptions<HidServiceOptions>();
            services.AddOptions<HeartbeatServiceOptions>();
            services.AddOptions<FileObserverServiceOptions>();
            services.AddOptions<CsvServiceOptions>();
            services.AddOptions<ScpServiceOptions>();
        }

        private static ServiceUiRegistration<ServiceListModel, Page> BuildMqttRegistration()
        {
            return new ServiceUiRegistration<ServiceListModel, Page>(
                ServiceType.Mqtt,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<MqttServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var mainViewModel = provider.GetRequiredService<MainViewModel>();
                    var newService = new ServiceListModel
                    {
                        DisplayName = $"MQTT - {ctx.Name}",
                        Type = ServiceType.Mqtt,
                        IsActive = false
                    };

                    mainView.GetOrCreateServicePage(newService);

                    var options = ctx.Options;
                    var resolved = provider.GetRequiredService<IOptions<MqttServiceOptions>>().Value;
                    resolved.Host = options.Host;
                    resolved.Port = options.Port;
                    resolved.ClientId = options.ClientId;
                    resolved.Username = options.Username;
                    resolved.Password = options.Password;
                    resolved.ConnectionType = options.ConnectionType;
                    resolved.WillTopic = options.WillTopic;
                    resolved.WillPayload = options.WillPayload;
                    resolved.WillQualityOfService = options.WillQualityOfService;
                    resolved.WillRetain = options.WillRetain;
                    resolved.KeepAliveSeconds = options.KeepAliveSeconds;
                    resolved.CleanSession = options.CleanSession;
                    resolved.ReconnectDelay = options.ReconnectDelay;

                    if (newService.ServicePage is MqttTagSubscriptionsView mqttView &&
                        mqttView.DataContext is MqttTagSubscriptionsViewModel mqttVm)
                    {
                        newService.ActiveChanged += active =>
                        {
                            if (active)
                            {
                                _ = mqttVm.ConnectAsync();
                            }
                        };

                        mqttVm.EditConnectionRequested += (_, _) =>
                        {
                            var editView = provider.GetRequiredService<MqttEditConnectionView>();
                            if (editView.DataContext is MqttEditConnectionViewModel vm)
                            {
                                var opt = provider.GetRequiredService<IOptions<MqttServiceOptions>>().Value;
                                vm.Load(opt);
                                vm.HighlightMissingFields();
                                vm.RequestClose += (_, _) =>
                                {
                                    if (newService.ServicePage != null)
                                    {
                                        mainView.ShowPage(newService.ServicePage);
                                    }

                                    _ = mainViewModel.SaveServicesAsync();
                                };
                            }

                            mainView.ShowPage(editView);
                        };
                    }

                    return newService;
                },
                provider => provider.GetRequiredService<MqttTagSubscriptionsView>(),
                (provider, defaultName) =>
                {
                    var vm = provider.GetRequiredService<MqttCreateServiceViewModel>();
                    vm.ServiceName = defaultName;
                    var mainView = provider.GetRequiredService<MainView>();
                    vm.ServiceSaved += (name, options) =>
                        _ = mainView.AddServiceAsync(ServiceType.Mqtt,
                            new ServiceFactoryOptions<MqttServiceOptions>(name, (MqttServiceOptions)options));
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    var view = ActivatorUtilities.CreateInstance<MqttCreateServiceView>(provider, vm);
                    vm.AdvancedConfigRequested += opts =>
                    {
                        var advVm = ActivatorUtilities.CreateInstance<MqttAdvancedConfigViewModel>(provider, opts);
                        var advView = provider.GetRequiredService<MqttAdvancedConfigView>();
                        advView.Initialize(advVm);
                        advVm.Saved += _ => mainView.ShowPage(view);
                        advVm.BackRequested += () => mainView.ShowPage(view);
                        mainView.ShowPage(advView);
                    };
                    return view;
                });
        }

        private static ServiceUiRegistration<ServiceListModel, Page> BuildFtpRegistration()
        {
            return new ServiceUiRegistration<ServiceListModel, Page>(
                ServiceType.Ftp,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<FtpServerOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var svc = new ServiceListModel
                    {
                        DisplayName = $"FTP Server - {ctx.Name}",
                        Type = ServiceType.Ftp,
                        IsActive = false,
                        FtpOptions = ctx.Options
                    };

                    mainView.GetOrCreateServicePage(svc);

                    var resolved = provider.GetRequiredService<IOptions<FtpServerOptions>>().Value;
                    resolved.Port = ctx.Options.Port;
                    resolved.RootPath = ctx.Options.RootPath;
                    resolved.AllowAnonymous = ctx.Options.AllowAnonymous;
                    resolved.Username = ctx.Options.Username;
                    resolved.Password = ctx.Options.Password;

                    return svc;
                },
                provider => provider.GetRequiredService<FTPServiceView>(),
                (provider, defaultName) =>
                {
                    var vm = provider.GetRequiredService<FtpServerCreateViewModel>();
                    vm.ServiceName = defaultName;
                    var mainView = provider.GetRequiredService<MainView>();
                    vm.ServiceSaved += (name, options) =>
                        _ = mainView.AddServiceAsync(ServiceType.Ftp,
                            new ServiceFactoryOptions<FtpServerOptions>(name, (FtpServerOptions)options));
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
                });
        }

        private static ServiceUiRegistration<ServiceListModel, Page> BuildHttpRegistration()
        {
            return new ServiceUiRegistration<ServiceListModel, Page>(
                ServiceType.Http,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<HttpServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var svc = new ServiceListModel
                    {
                        DisplayName = $"{ServiceType.Http.ToLegacyString()} - {ctx.Name}",
                        Type = ServiceType.Http,
                        IsActive = false,
                        HttpOptions = ctx.Options
                    };

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
                        _ = mainView.AddServiceAsync(ServiceType.Http,
                            new ServiceFactoryOptions<HttpServiceOptions>(name, (HttpServiceOptions)options));
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
                });
        }

        private static ServiceUiRegistration<ServiceListModel, Page> BuildTcpRegistration()
        {
            return new ServiceUiRegistration<ServiceListModel, Page>(
                ServiceType.Tcp,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<TcpServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var svc = new ServiceListModel
                    {
                        DisplayName = $"{ServiceType.Tcp.ToLegacyString()} - {ctx.Name}",
                        Type = ServiceType.Tcp,
                        IsActive = false,
                        TcpOptions = ctx.Options
                    };

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
                        _ = mainView.AddServiceAsync(ServiceType.Tcp,
                            new ServiceFactoryOptions<TcpServiceOptions>(name, (TcpServiceOptions)options));
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    return ActivatorUtilities.CreateInstance<TcpCreateServiceView>(provider, vm);
                });
        }

        private static ServiceUiRegistration<ServiceListModel, Page> BuildHidRegistration()
        {
            return new ServiceUiRegistration<ServiceListModel, Page>(
                ServiceType.Hid,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<HidServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var svc = new ServiceListModel
                    {
                        DisplayName = $"{ServiceType.Hid.ToLegacyString()} - {ctx.Name}",
                        Type = ServiceType.Hid,
                        IsActive = false,
                        HidOptions = ctx.Options
                    };

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
                        _ = mainView.AddServiceAsync(ServiceType.Hid,
                            new ServiceFactoryOptions<HidServiceOptions>(name, (HidServiceOptions)options));
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
                });
        }

        private static ServiceUiRegistration<ServiceListModel, Page> BuildScpRegistration()
        {
            return new ServiceUiRegistration<ServiceListModel, Page>(
                ServiceType.Scp,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<ScpServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var svc = new ServiceListModel
                    {
                        DisplayName = $"{ServiceType.Scp.ToLegacyString()} - {ctx.Name}",
                        Type = ServiceType.Scp,
                        IsActive = false,
                        ScpOptions = ctx.Options
                    };

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
                        _ = mainView.AddServiceAsync(ServiceType.Scp,
                            new ServiceFactoryOptions<ScpServiceOptions>(name, (ScpServiceOptions)options));
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
                });
        }

        private static ServiceUiRegistration<ServiceListModel, Page> BuildFileObserverRegistration()
        {
            return new ServiceUiRegistration<ServiceListModel, Page>(
                ServiceType.FileObserver,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<FileObserverServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var svc = new ServiceListModel
                    {
                        DisplayName = $"{ServiceType.FileObserver.ToLegacyString()} - {ctx.Name}",
                        Type = ServiceType.FileObserver,
                        IsActive = false,
                        FileObserverOptions = ctx.Options
                    };

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
                        _ = mainView.AddServiceAsync(ServiceType.FileObserver,
                            new ServiceFactoryOptions<FileObserverServiceOptions>(name, (FileObserverServiceOptions)options));
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
                });
        }

        private static ServiceUiRegistration<ServiceListModel, Page> BuildCsvRegistration()
        {
            return new ServiceUiRegistration<ServiceListModel, Page>(
                ServiceType.Csv,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<CsvServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var svc = new ServiceListModel
                    {
                        DisplayName = $"{ServiceType.Csv.ToLegacyString()} - {ctx.Name}",
                        Type = ServiceType.Csv,
                        IsActive = false,
                        CsvOptions = ctx.Options
                    };

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
                        _ = mainView.AddServiceAsync(ServiceType.Csv,
                            new ServiceFactoryOptions<CsvServiceOptions>(name, (CsvServiceOptions)options));
                    vm.EditCancelled += mainView.ShowCreateServiceSelectionPage;
                    var view = provider.GetRequiredService<CsvServiceEditorView>();
                    view.Initialize(vm);
                    vm.AdvancedConfigRequested += opts =>
                    {
                        var advVm = ActivatorUtilities.CreateInstance<CsvAdvancedConfigViewModel>(provider, opts);
                        var advView = provider.GetRequiredService<CsvAdvancedConfigView>();
                        advView.Initialize(advVm);
                        advVm.Saved += _ => mainView.ShowPage(view);
                        advVm.BackRequested += () => mainView.ShowPage(view);
                        mainView.ShowPage(advView);
                    };
                    return view;
                });
        }

        private static ServiceUiRegistration<ServiceListModel, Page> BuildHeartbeatRegistration()
        {
            return new ServiceUiRegistration<ServiceListModel, Page>(
                ServiceType.Heartbeat,
                (provider, optionsObj) =>
                {
                    var ctx = (ServiceFactoryOptions<HeartbeatServiceOptions>)optionsObj;
                    var mainView = provider.GetRequiredService<MainView>();
                    var svc = new ServiceListModel
                    {
                        DisplayName = $"{ServiceType.Heartbeat.ToLegacyString()} - {ctx.Name}",
                        Type = ServiceType.Heartbeat,
                        IsActive = false,
                        HeartbeatOptions = ctx.Options
                    };

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
                        _ = mainView.AddServiceAsync(ServiceType.Heartbeat,
                            new ServiceFactoryOptions<HeartbeatServiceOptions>(name, (HeartbeatServiceOptions)options));
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
                });
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

            KeyboardSimulator.Reset();
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
