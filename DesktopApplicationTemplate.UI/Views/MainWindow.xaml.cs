using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.ComponentModel;
using DesktopApplicationTemplate.Services;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Tcp;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt;
using DesktopApplicationTemplate.UI.ViewModels.Mqtt.Edit;
using DesktopApplicationTemplate.UI.Views.Mqtt;
using DesktopApplicationTemplate.UI.Views.Mqtt.Edit;
using DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;
using LogLevel = DesktopApplicationTemplate.Core.Services.LogLevel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.VisualStudio.Threading;

namespace DesktopApplicationTemplate.UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [SupportedOSPlatform("windows")]
    public partial class MainView : Window
    {
        private const string LogExportDialogFilter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt|All files (*.*)|*.*";
        private const string LogExportTimestampFormat = "yyyyMMdd_HHmmss";
        private const string LogExportDefaultPrefix = "AllServices";

        private readonly MainViewModel _viewModel;
        private readonly ILogger<MainView>? _logger;
        private readonly IServiceUiRegistry<ServiceListModel, Page> _serviceRegistry;
        private readonly IServiceCatalog _serviceCatalog;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<ServiceListModel, Action<LogEntry>> _serviceLogHandlers = new();
        private readonly Dictionary<ServiceListModel, ServiceLoggingAdapter> _serviceLogAdapters = new();
        private readonly Dictionary<ServiceListModel, EventHandler> _tcpAdvancedHandlers = new();
        private readonly Dictionary<ServiceListModel, EventHandler> _mqttEditHandlers = new();
        private readonly Dictionary<ServiceListModel, MqttTagSubscriptionsViewModel> _mqttSubscriptionViewModels = new();
        private readonly BrushConverter _brushConverter = new();
        private readonly IDictionary<ServiceType, IEditServiceHandler> _editHandlers;
        private JoinableTask? _shutdownTask;
        private bool _shutdownCompleted;

        public MainView(
            MainViewModel viewModel,
            IServiceUiRegistry<ServiceListModel, Page> serviceRegistry,
            IServiceProvider serviceProvider,
            IServiceCatalog serviceCatalog)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _serviceRegistry = serviceRegistry;
            _serviceProvider = serviceProvider;
            _editHandlers = serviceProvider.GetService<IDictionary<ServiceType, IEditServiceHandler>>()
                ?? new Dictionary<ServiceType, IEditServiceHandler>();
            _serviceCatalog = serviceCatalog ?? throw new ArgumentNullException(nameof(serviceCatalog));
            if (_serviceProvider.GetService(typeof(ILoggerFactory)) is ILoggerFactory factory)
            {
                _logger = factory.CreateLogger<MainView>();
            }
            DataContext = _viewModel;
            _viewModel.ConfigurationChangeBlocked += OnConfigurationChangeBlocked;
            _viewModel.AddServiceRequested += OnAddServiceRequested;
            _viewModel.ExportPluginsRequested += OnExportPluginsRequested;
            _viewModel.HomeRequested += OnHomeRequested;
            _viewModel.Services.CollectionChanged += Services_CollectionChanged;
            MouseDown += MainView_MouseDown;
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseCommand_Executed));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeCommand_Executed));
            Closing += MainView_Closing;
            Closed += MainView_Closed;
            ShowHome();
            PreloadServicePages();
        }

        private void MainView_Closing(object? sender, CancelEventArgs e)
        {
            _logger?.LogInformation("MainView closing");

            if (_shutdownCompleted)
            {
                return;
            }

            if (_shutdownTask is { IsCompleted: false })
            {
                e.Cancel = true;
                return;
            }

            e.Cancel = true;
            _shutdownTask = App.UiThreadTaskFactory.RunAsync(PerformShutdownAsync);
            ObserveAndLog(_shutdownTask);
        }

        private async Task PerformShutdownAsync()
        {
            try
            {
                await _viewModel.ShutdownServicesAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to stop services during shutdown");
            }
            finally
            {
                await App.UiThreadTaskFactory.SwitchToMainThreadAsync();
                _shutdownCompleted = true;
                _shutdownTask = null;
                Close();
            }
        }

        private void MainView_Closed(object? sender, EventArgs e)
        {
            _viewModel.ConfigurationChangeBlocked -= OnConfigurationChangeBlocked;
            _viewModel.ExportPluginsRequested -= OnExportPluginsRequested;
            _viewModel.HomeRequested -= OnHomeRequested;
        }

        public void ShowHome()
        {
            ContentFrame.Content = null;
            ContentFrame.Visibility = Visibility.Collapsed;
            HomeContentGrid.Visibility = Visibility.Visible;
        }

        public void ShowPage(Page page)
        {
            HomeContentGrid.Visibility = Visibility.Collapsed;
            ContentFrame.Visibility = Visibility.Visible;
            ContentFrame.Content = page;
        }

        private void OnConfigurationChangeBlocked()
        {
            MessageBox.Show(this,
                "Stop the active services before making any changes.",
                "Services Running",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _logger?.LogInformation("Close command invoked");
            Close();
        }

        private void MinimizeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _logger?.LogInformation("Minimize command invoked");
            SystemCommands.MinimizeWindow(this);
        }

        private void OnExportPluginsRequested(object? sender, EventArgs e)
        {
            if (_serviceProvider.GetService(typeof(PluginExportWindow)) is not PluginExportWindow exportWindow)
            {
                _logger?.LogWarning("Plugin export window could not be resolved from the service provider.");
                return;
            }

            exportWindow.Owner = this;
            exportWindow.ShowDialog();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e) => NavigateHome("Home button");

        private void AppTitle_Click(object sender, RoutedEventArgs e)
        {
            NavigateHome("Application title");
            e.Handled = true;
        }

        private void NavigateHome(string source)
        {
            _logger?.LogInformation("{Source} clicked", source);
            _viewModel.SelectedService = null;
            ServiceList.SelectedItem = null;
            ShowHome();
        }

        private void OnHomeRequested(object? sender, string reason)
        {
            if (Dispatcher.CheckAccess())
            {
                NavigateHome(reason);
                return;
            }

            var joinableTask = App.UiThreadTaskFactory.RunAsync(async () =>
            {
                await App.UiThreadTaskFactory.SwitchToMainThreadAsync();
                NavigateHome(reason);
            });

            ObserveAndLog(joinableTask);
        }


        public Page? GetOrCreateServicePage(ServiceListModel svc)
        {
            if (svc.ServicePage == null && _serviceRegistry.TryCreateServicePage(svc.Type, _serviceProvider, out var page))
            {
                svc.ServicePage = page;
            }

            if (svc.ServicePage != null)
            {
                if (svc.Type == ServiceType.Mqtt && svc.ServicePage is MqttTagSubscriptionsView mqttPage)
                {
                    InitializeMqttSubscriptionsPage(svc, mqttPage);
                }

                if (svc.ServicePage.DataContext is ILoggingViewModel vm)
                {
                    AttachServiceLogger(svc, vm);
                }

                if (svc.ServicePage.DataContext is TcpServiceMessagesViewModel tcpVm)
                {
                    AttachTcpAdvancedHandler(svc, tcpVm);
                }

                if (svc.ServicePage.DataContext is INetworkAwareViewModel navm)
                {
                    navm.UpdateNetworkConfiguration(_viewModel.NetworkConfig.CurrentConfiguration);
                }

                if (svc.ServicePage is IServiceLogHost logHost)
                {
                    logHost.SetServiceContext(svc);
                }
            }

            return svc.ServicePage;
        }

        private void AttachServiceLogger(ServiceListModel svc, ILoggingViewModel vm)
        {
            if (_serviceLogHandlers.ContainsKey(svc))
            {
                return;
            }

            var baseLogger = vm.Logger ?? _serviceProvider.GetRequiredService<ILoggingService>();
            ServiceLoggingAdapter adapter;

            if (baseLogger is ServiceLoggingAdapter existingAdapter)
            {
                adapter = existingAdapter;
            }
            else
            {
                adapter = new ServiceLoggingAdapter(baseLogger, svc.Type, () => svc.DisplayName);
            }

            vm.Logger = adapter;
            _serviceLogAdapters[svc] = adapter;

            Action<LogEntry> handler = entry =>
            {
                void ApplyLog()
                {
                    if (svc.Type == ServiceType.Mqtt)
                    {
                        _viewModel.OnServiceLogAdded(svc, entry);
                        return;
                    }

                    Brush? brush = null;
                    try
                    {
                        brush = _brushConverter.ConvertFromString(entry.Color) as Brush;
                    }
                    catch
                    {
                        brush = null;
                    }

                    var message = entry.Message;
                    if (entry.ServiceType == svc.Type && !string.IsNullOrWhiteSpace(entry.ServiceName))
                    {
                        var contextToken = $"[{entry.ServiceType}.{entry.ServiceName}] ";
                        var tokenIndex = message.IndexOf(contextToken, StringComparison.OrdinalIgnoreCase);
                        if (tokenIndex >= 0)
                        {
                            message = message.Remove(tokenIndex, contextToken.Length);
                        }
                    }

                    svc.AddLog(message, brush ?? Brushes.Black, entry.Level);
                }

                if (Dispatcher.CheckAccess())
                {
                    ApplyLog();
                }
                else
                {
                    App.UiThreadTaskFactory.Run(async () =>
                    {
                        await App.UiThreadTaskFactory.SwitchToMainThreadAsync();
                        ApplyLog();
                    });
                }
            };

            vm.Logger.LogAdded += handler;
            _serviceLogHandlers[svc] = handler;

            if (svc.Logs.Count == 0)
            {
                ObserveTask(vm.Logger.ReloadAsync());
            }
        }

        private void InitializeMqttSubscriptionsPage(ServiceListModel svc, MqttTagSubscriptionsView page)
        {
            if (svc is null || page is null)
            {
                return;
            }

            if (!_mqttSubscriptionViewModels.TryGetValue(svc, out var viewModel))
            {
                viewModel = ActivatorUtilities.CreateInstance<MqttTagSubscriptionsViewModel>(
                    _serviceProvider,
                    svc);
                _mqttSubscriptionViewModels[svc] = viewModel;
            }

            if (_mqttEditHandlers.TryGetValue(svc, out var existingHandler))
            {
                viewModel.EditConnectionRequested -= existingHandler;
            }

            EventHandler handler = (_, _) => ShowMqttEditConnectionView(svc, highlightMissingFields: true);
            viewModel.EditConnectionRequested += handler;
            _mqttEditHandlers[svc] = handler;

            _ = svc.GetOrCreateOptions(() => new MqttServiceOptions());

            page.Initialize(svc, viewModel);
        }

        private void AttachTcpAdvancedHandler(ServiceListModel svc, TcpServiceMessagesViewModel vm)
        {
            if (_tcpAdvancedHandlers.TryGetValue(svc, out var existing))
            {
                vm.AdvancedSettingsRequested -= existing;
            }

            EventHandler handler = (_, _) => OpenTcpAdvancedSettings(svc);
            vm.AdvancedSettingsRequested += handler;
            _tcpAdvancedHandlers[svc] = handler;
        }

        private void DetachServiceLogger(ServiceListModel svc)
        {
            if (_serviceLogHandlers.TryGetValue(svc, out var handler))
            {
                if (svc.ServicePage?.DataContext is ILoggingViewModel vm && vm.Logger is not null)
                {
                    vm.Logger.LogAdded -= handler;
                }

                _serviceLogHandlers.Remove(svc);
            }

            if (_serviceLogAdapters.Remove(svc, out var adapter))
            {
                if (svc.ServicePage?.DataContext is ILoggingViewModel vm)
                {
                    vm.Logger = adapter.InnerLogger;
                }

                adapter.Dispose();
            }

            DetachTcpAdvancedHandler(svc);
            DetachMqttHandlers(svc);
        }

        private void DetachTcpAdvancedHandler(ServiceListModel svc)
        {
            if (!_tcpAdvancedHandlers.TryGetValue(svc, out var handler))
            {
                return;
            }

            if (svc.ServicePage?.DataContext is TcpServiceMessagesViewModel vm)
            {
                vm.AdvancedSettingsRequested -= handler;
            }

            _tcpAdvancedHandlers.Remove(svc);
        }

        private void DetachMqttHandlers(ServiceListModel svc)
        {
            if (_mqttEditHandlers.TryGetValue(svc, out var handler) &&
                _mqttSubscriptionViewModels.TryGetValue(svc, out var viewModel))
            {
                viewModel.EditConnectionRequested -= handler;
            }

            _mqttEditHandlers.Remove(svc);
            _mqttSubscriptionViewModels.Remove(svc);

            var sessionManager = _serviceProvider.GetRequiredService<IMqttClientSessionManager>();
            ObserveTask(sessionManager.ReleaseAsync(svc));
        }

        private void OpenTcpAdvancedSettings(ServiceListModel svc)
        {
            if (_editHandlers.TryGetValue(ServiceType.Tcp, out var handler))
            {
                handler.Edit(svc);
            }
        }

        private void ShowMqttEditConnectionView(ServiceListModel service, bool highlightMissingFields = false)
        {
            if (service is null)
            {
                return;
            }

            _ = service.GetOrCreateOptions(() => new MqttServiceOptions());

            var logger = _serviceProvider.GetService<ILoggingService>();
            var viewModel = logger is null
                ? ActivatorUtilities.CreateInstance<MqttEditConnectionViewModel>(
                    _serviceProvider,
                    service)
                : ActivatorUtilities.CreateInstance<MqttEditConnectionViewModel>(
                    _serviceProvider,
                    service,
                    logger);

            if (highlightMissingFields)
            {
                viewModel.HighlightMissingFields();
            }

            var editView = _serviceProvider.GetRequiredService<MqttEditConnectionView>();
            editView.Initialize(viewModel);

            EventHandler? closeHandler = null;
            closeHandler = (_, _) =>
            {
                viewModel.RequestClose -= closeHandler;
                if (service.ServicePage != null)
                {
                    ShowPage(service.ServicePage);
                }

                ObserveTask(_viewModel.SaveServicesAsync());
            };
            viewModel.RequestClose += closeHandler;

            ShowPage(editView);
        }

        private void Services_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace)
            {
                foreach (ServiceListModel svc in e.OldItems?.OfType<ServiceListModel>() ?? Enumerable.Empty<ServiceListModel>())
                {
                    DetachServiceLogger(svc);
                }
            }

            if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Replace)
            {
                foreach (ServiceListModel svc in e.NewItems?.OfType<ServiceListModel>() ?? Enumerable.Empty<ServiceListModel>())
                {
                    GetOrCreateServicePage(svc);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var svc in _serviceLogHandlers.Keys.ToList())
                {
                    DetachServiceLogger(svc);
                }
                foreach (var svc in _tcpAdvancedHandlers.Keys.ToList())
                {
                    DetachTcpAdvancedHandler(svc);
                }
                foreach (var svc in _mqttEditHandlers.Keys.ToList())
                {
                    DetachMqttHandlers(svc);
                }
            }
        }

        private void PreloadServicePages()
        {
            foreach (var svc in _viewModel.Services)
            {
                GetOrCreateServicePage(svc);
            }
        }

        private void AddService_Click(object sender, RoutedEventArgs e)
        {
            _logger?.LogDebug("AddService button clicked");
            if (_viewModel.AddServiceCommand.CanExecute(null))
            {
                _viewModel.AddServiceCommand.Execute(null);
            }
        }

        private void OnAddServiceRequested()
        {
            ShowCreateServiceSelectionPage();
        }

        public void ShowCreateServiceSelectionPage()
        {
            var page = _serviceProvider.GetRequiredService<CreateServicePage>();
            _createServicePage = page;
            page.SetExistingNames(_viewModel.Services.Select(s => s.DisplayName));
            page.ServiceCreated += async (name, type) =>
            {
                using var creationScope = _viewModel.BeginServiceCreationScope();
                var trimmed = string.IsNullOrWhiteSpace(name)
                    ? _viewModel.GenerateServiceName(type)
                    : name.Trim();
                if (_viewModel.Services.Any(s => s.DisplayName.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
                {
                    trimmed = _viewModel.GenerateServiceName(type);
                }

                _viewModel.ClearRoutingCache(type, trimmed);
                var routing = _serviceProvider.GetRequiredService<IMessageRoutingService>();
                var svc = new ServiceListModel(routing)
                {
                    DisplayName = trimmed,
                    Type = type
                };
                svc.InitializeActivationState(isActive: false);
                ApplyPresentation(svc);
                svc.LogAdded += _viewModel.OnServiceLogAdded;
                svc.ActiveChanged += _viewModel.OnServiceActiveChanged;
                GetOrCreateServicePage(svc);
                _viewModel.Services.Add(svc);
                _logger?.LogInformation("Service {Name} added", svc.DisplayName);
                _viewModel.SelectedService = svc;
                ServiceList.ScrollIntoView(svc);
                if (svc.ServicePage != null)
                    ShowPage(svc.ServicePage);
                await _viewModel.SaveServicesAsync().ConfigureAwait(false);
                page.SetExistingNames(_viewModel.Services.Select(s => s.DisplayName));
            };
            page.ServiceTypeSelected += NavigateTo;
            page.Cancelled += ShowHome;
            ShowPage(page);
        }

        private CreateServicePage? _createServicePage;

        private void ApplyPresentation(ServiceListModel service)
        {
            if (service is null)
            {
                return;
            }

            var descriptorId = service.DescriptorId;
            if (string.IsNullOrWhiteSpace(descriptorId))
            {
                descriptorId = _serviceCatalog.Descriptors.FirstOrDefault(d => d.ServiceType == service.Type)?.Id;
                service.DescriptorId = descriptorId;
            }

            if (!string.IsNullOrWhiteSpace(descriptorId) &&
                _serviceCatalog.TryGetById(descriptorId, out var descriptor))
            {
                service.ApplyPresentation(descriptor.Presentation);
            }
            else
            {
                service.ApplyPresentation(ServicePresentationMetadata.Empty);
            }
        }

        private void NavigateTo(ServiceType serviceType)
        {
            var defaultName = _createServicePage?.GenerateDefaultName(serviceType) ?? serviceType.ToBaseName();
            if (_serviceRegistry.TryCreateNavigationPage(serviceType, _serviceProvider, defaultName, out var view) &&
                view is not null)
            {
                ShowPage(view);
            }
        }









        internal async Task<bool> TryAddServiceAsync<TOptions>(ServiceType type, ServiceFactoryOptions<TOptions> context)
        {
            using var creationScope = _viewModel.BeginServiceCreationScope();
            var sanitizedName = string.IsNullOrWhiteSpace(context.Name)
                ? _viewModel.GenerateServiceName(type)
                : context.Name.Trim();

            if (_viewModel.Services.Any(s => s.DisplayName.Equals(sanitizedName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show(this,
                    $"A service named '{sanitizedName}' already exists.",
                    "Duplicate Service Name",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }

            var normalizedContext = context with { Name = sanitizedName };

            _viewModel.ClearRoutingCache(type, sanitizedName);

            if (!_serviceRegistry.TryCreateService(type, _serviceProvider, normalizedContext, out var svc) || svc is null)
            {
                return false;
            }

            svc.InitializeActivationState(isActive: false);
            ApplyPresentation(svc);
            svc.LogAdded += _viewModel.OnServiceLogAdded;
            svc.ActiveChanged += _viewModel.OnServiceActiveChanged;

            GetOrCreateServicePage(svc);

            _viewModel.Services.Add(svc);
            _logger?.LogInformation("Service {Name} added", svc.DisplayName);
            _viewModel.SelectedService = svc;
            ServiceList.ScrollIntoView(svc);
            if (svc.ServicePage != null)
            {
                ShowPage(svc.ServicePage);
            }

            await _viewModel.SaveServicesAsync();
            _createServicePage?.SetExistingNames(_viewModel.Services.Select(s => s.DisplayName));
            _logger?.LogDebug("AddService workflow completed");
            return true;
        }


        private void RemoveService_Click(object sender, RoutedEventArgs e)
        {
            _logger?.LogDebug("RemoveService button clicked");
            if (DataContext is ViewModels.MainViewModel vm)
            {
                if (vm.HasMarkedServices && vm.RemoveMarkedServicesCommand.CanExecute(null))
                {
                    vm.RemoveMarkedServicesCommand.Execute(null);
                    _logger?.LogDebug("RemoveMarkedServices command executed");
                    return;
                }

                var target = vm.SelectedService ?? vm.ActiveService;
                if (vm.RemoveServiceCommand.CanExecute(target))
                {
                    vm.RemoveServiceCommand.Execute(target);
                    _logger?.LogDebug("RemoveService command executed");
                }
            }
        }
        private void ServiceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _logger?.LogDebug("Service selection changed");
            if (_viewModel.SelectedService is ServiceListModel selected)
            {
                var page = GetOrCreateServicePage(selected);
                if (page != null)
                {
                    ShowPage(page);
                    using (_viewModel.PreserveActiveServiceSelection())
                    {
                        ServiceList.SelectedItem = null;
                    }
                }

                return;
            }

            if (_viewModel.ActiveService is null)
            {
                ShowHome();
            }
        }

        private void ServiceList_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.Handled || e.ClickCount != 1)
            {
                return;
            }

            if (e.OriginalSource is not DependencyObject source)
            {
                return;
            }

            var item = Helpers.VisualTreeHelperExtensions.FindParent<ListBoxItem>(source);
            if (item?.DataContext is not ServiceListModel svc)
            {
                return;
            }

            if (!ReferenceEquals(svc, _viewModel.SelectedService) && !ReferenceEquals(svc, _viewModel.ActiveService))
            {
                return;
            }

            var page = GetOrCreateServicePage(svc);
            if (page != null)
            {
                ShowPage(page);
            }
        }

        private void ServiceList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            if (e.OriginalSource is not DependencyObject source)
            {
                return;
            }

            var item = Helpers.VisualTreeHelperExtensions.FindParent<ListBoxItem>(source);
            if (item?.DataContext is not ServiceListModel svc)
            {
                return;
            }

            _logger?.LogDebug("Service {Name} double-clicked", svc.DisplayName);
            if (_viewModel.EditServiceCommand.CanExecute(svc))
            {
                _viewModel.EditServiceCommand.Execute(svc);
                e.Handled = true;
            }
        }

        private void ServiceListItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is not ListBoxItem { DataContext: ServiceListModel svc })
            {
                return;
            }

            _logger?.LogDebug("Service {Name} double-clicked via item container", svc.DisplayName);
            if (_viewModel.EditServiceCommand.CanExecute(svc))
            {
                _viewModel.EditServiceCommand.Execute(svc);
                e.Handled = true;
            }
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilterPopup != null)
            {
                FilterPopup.IsOpen = !FilterPopup.IsOpen;
            }
        }

        internal void DeleteServiceMenu_Click(object sender, RoutedEventArgs e) => _ = DeleteServiceMenuAsync(sender, e);

        private async Task DeleteServiceMenuAsync(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem { DataContext: ServiceListModel svc })
            {
                if (!_viewModel.RequestConfigurationChange())
                {
                    return;
                }

                var index = _viewModel.Services.IndexOf(svc);
                svc.LogAdded -= _viewModel.OnServiceLogAdded;
                _viewModel.RemoveServiceAssociations(svc);
                _viewModel.Services.Remove(svc);
                if (_viewModel.Services.Count > 0)
                {
                    if (index >= _viewModel.Services.Count) index = _viewModel.Services.Count - 1;
                    _viewModel.SelectedService = _viewModel.Services[index];
                }
                else
                {
                    _viewModel.SelectedService = null;
                }
                await _viewModel.SaveServicesAsync();
                _createServicePage?.SetExistingNames(_viewModel.Services.Select(s => s.DisplayName));
            }
        }

        internal void RenameServiceMenu_Click(object sender, RoutedEventArgs e) => _ = RenameServiceMenuAsync(sender, e);

        private async Task RenameServiceMenuAsync(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem { DataContext: ServiceListModel svc })
            {
                if (!_viewModel.RequestConfigurationChange())
                {
                    return;
                }

                string input = Microsoft.VisualBasic.Interaction.InputBox("Enter new service name:", "Rename Service", svc.DisplayName);
                if (!string.IsNullOrWhiteSpace(input))
                {
                    var trimmed = input.Trim();
                    if (_viewModel.Services.Any(s => s != svc && s.DisplayName.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
                    {
                        trimmed = _viewModel.GenerateServiceName(svc.Type);
                    }
                    if (!string.Equals(svc.DisplayName, trimmed, StringComparison.Ordinal))
                    {
                        _viewModel.ClearRoutingCache(svc.Type, svc.DisplayName);
                        _viewModel.ClearRoutingCache(svc.Type, trimmed);
                        svc.DisplayName = trimmed;
                    }
                    await _viewModel.SaveServicesAsync();
                    _createServicePage?.SetExistingNames(_viewModel.Services.Select(s => s.DisplayName));
                }
            }
        }

        internal void ChangeColorMenu_Click(object sender, RoutedEventArgs e) => _ = ChangeColorMenuAsync(sender, e);

        private async Task ChangeColorMenuAsync(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem { DataContext: ServiceListModel svc })
            {
                if (!_viewModel.RequestConfigurationChange())
                {
                    return;
                }

                var dlg = new ColorPickerWindow { Owner = this };
                if (dlg.ShowDialog() == true)
                {
                    var color = dlg.ChosenColor;
                    var brush = new SolidColorBrush(color);
                    foreach (var s in _viewModel.Services.Where(s => s.Type == svc.Type))
                    {
                        s.BackgroundColor = brush;
                        s.BorderColor = brush;
                    }
                    await _viewModel.SaveServicesAsync();
                }
            }
        }

        private void HeaderBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                HeaderBar_MouseDoubleClick(sender, e);
                e.Handled = true;
                return;
            }

            if (e.OriginalSource is not DependencyObject element ||
                Helpers.VisualTreeHelperExtensions.FindParent<ButtonBase>(element) == null)
            {
                try
                {
                    DragMove();
                }
                catch (InvalidOperationException ex)
                {
                    _logger?.LogWarning(ex, "DragMove failed");
                }
            }
            e.Handled = true;
        }

        private void HeaderBar_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject element &&
                Helpers.VisualTreeHelperExtensions.FindParent<ButtonBase>(element) != null)
                return;

            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;

            _logger?.LogInformation("Window state changed to {State}", WindowState);
            e.Handled = true;
        }

        private void MainView_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource is not DependencyObject element)
                return;

            bool clickedListItem = Helpers.VisualTreeHelperExtensions.FindParent<ListBoxItem>(element) != null;
            bool clickedFrame = Helpers.VisualTreeHelperExtensions.FindParent<Frame>(element) != null;
            bool clickedButton = Helpers.VisualTreeHelperExtensions.FindParent<ButtonBase>(element) != null;

            if (e.ChangedButton == System.Windows.Input.MouseButton.Left && !clickedListItem && !clickedFrame && !clickedButton)
            {
                try
                {
                    DragMove();
                }
                catch (InvalidOperationException ex)
                {
                    _logger?.LogWarning(ex, "DragMove failed");
                }
            }

        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.RequestConfigurationChange())
            {
                return;
            }

            var page = _serviceProvider.GetRequiredService<SettingsPage>();
            ShowPage(page);
        }

        private System.Windows.Point _dragStart;
        internal void ServiceItem_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _dragStart = e.GetPosition(null);
        }

        internal void ServiceItem_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is not DependencyObject source)
            {
                return;
            }

            var item = Helpers.VisualTreeHelperExtensions.FindParent<ListBoxItem>(source);
            if (item != null)
            {
                item.Focus();
                item.IsSelected = true;
            }

            ServiceList.Focus();
        }

        internal void ServiceItem_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed)
                return;

            var position = e.GetPosition(null);
            if (Math.Abs(position.X - _dragStart.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(position.Y - _dragStart.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                if (sender is Border { DataContext: ServiceListModel svc } border)
                {
                    DragDrop.DoDragDrop(border, svc, System.Windows.DragDropEffects.Move);
                }
            }
        }

        internal void ServiceItem_Drop(object sender, System.Windows.DragEventArgs e) => _ = ServiceItemDropAsync(sender, e);

        private async Task ServiceItemDropAsync(object sender, System.Windows.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(ServiceListModel)))
                return;

            if (!_viewModel.RequestConfigurationChange())
            {
                e.Handled = true;
                return;
            }

            var source = (ServiceListModel)e.Data.GetData(typeof(ServiceListModel))!;
            if (sender is not Border { DataContext: ServiceListModel target } || source == target)
                return;

            int oldIndex = _viewModel.Services.IndexOf(source);
            int newIndex = _viewModel.Services.IndexOf(target);
            if (oldIndex != newIndex)
            {
                _viewModel.Services.Move(oldIndex, newIndex);
                await _viewModel.SaveServicesAsync();
            }
        }

        // Legacy handler removed; log level selection is now managed by ServiceLogView.

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            _logger?.LogInformation("Clear log button clicked");
            _viewModel.ClearLogs();
        }

        private void ExportLog_Click(object sender, RoutedEventArgs e)
        {
            _logger?.LogInformation("Home view export logs button clicked");

            var button = sender as Button;
            if (button is not null)
            {
                button.IsEnabled = false;
            }

            ObserveTask(ExportLogsAsync(button));
        }

        private async Task ExportLogsAsync(Button? button)
        {
            var timestamp = DateTime.Now.ToString(LogExportTimestampFormat);
            var suggestedName = $"{LogExportDefaultPrefix}_{timestamp}.log";
            var selectedPath = App.FileDialogService.SaveFile(suggestedName, LogExportDialogFilter);

            try
            {
                if (string.IsNullOrWhiteSpace(selectedPath))
                {
                    _logger?.LogInformation("Log export canceled by the user.");
                    return;
                }

                var (success, errorMessage) = await _viewModel.TryExportAllLogsAsync(selectedPath).ConfigureAwait(false);
                if (success)
                {
                    _logger?.LogInformation("All logs exported to {FilePath}", selectedPath);
                    await App.UiThreadTaskFactory.SwitchToMainThreadAsync();
                    MessageBox.Show(
                        this,
                        $"Logs exported to:\n{selectedPath}",
                        "Export Complete",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    var failureMessage = string.IsNullOrWhiteSpace(errorMessage)
                        ? "An unknown error occurred."
                        : errorMessage;
                    _logger?.LogError("Failed to export logs to {FilePath}: {ErrorMessage}", selectedPath, failureMessage);
                    await App.UiThreadTaskFactory.SwitchToMainThreadAsync();
                    MessageBox.Show(
                        this,
                        $"Failed to export logs to '{selectedPath}': {failureMessage}",
                        "Export Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            finally
            {
                if (button is not null)
                {
                    await App.UiThreadTaskFactory.SwitchToMainThreadAsync();
                    button.IsEnabled = true;
                }
            }
        }

        private void RefreshLog_Click(object sender, RoutedEventArgs e)
        {
            _logger?.LogDebug("Refresh log button clicked");
            _viewModel.RefreshLogs();
        }

        private void ObserveAndLog(JoinableTask? joinableTask)
        {
            if (joinableTask is null)
            {
                return;
            }

            async Task ObserveBackgroundTaskAsync()
            {
                try
                {
                    await joinableTask.JoinAsync();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Unhandled exception in background operation");
                }
            }

            _ = ObserveBackgroundTaskAsync();
        }

        private static void ObserveTask(Task? task)
        {
            if (task is null)
            {
                return;
            }

            _ = task.ContinueWith(
                t => _ = t.Exception,
                System.Threading.CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

    }
}
