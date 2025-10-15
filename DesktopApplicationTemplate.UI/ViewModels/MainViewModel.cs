using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using WpfBrushes = System.Windows.Media.Brushes;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Persistence;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels.Tcp;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI;
using DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;
using Microsoft.Extensions.Options;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public interface IServiceLookup
    {
        bool TryGetService(ServiceType type, string name, out ServiceListModel? service);

        IEnumerable<ServiceListModel> FindByDisplayName(string name);
    }

    public partial class MainViewModel : ViewModelBase, IServiceLookup
    {
        private const int DefaultAggregatedLogCapacity = 1000;

        public ObservableCollection<ServiceListModel> Services { get; set; } = new();
        public ICollectionView FilteredServices { get; }
        public FilterViewModel Filters { get; } = new();
        public LimitedObservableCollection<LogEntry> AllLogs { get; }
        private ServiceListModel? _selectedService;
        private ServiceListModel? _activeService;
        public ServiceListModel? SelectedService
        {
            get => _selectedService;
            set
            {
                _selectedService = value;
                OnPropertyChanged();
                ActiveService = value;
            }
        }
        public ServiceListModel? ActiveService
        {
            get => _activeService;
            private set
            {
                if (_activeService == value)
                {
                    return;
                }

                _activeService = value;
                OnPropertyChanged();
                LogViewModel.SetLogs(
                    _activeService?.LogState.Logs ?? AllLogs,
                    _activeService is null,
                    newestFirstInSource: _activeService is not null);
                RefreshServiceCommandStates();
            }
        }
        public ICommand AddServiceCommand { get; }
        public ICommand RemoveServiceCommand { get; }
        public ICommand EditServiceCommand { get; }
        public ICommand ToggleServiceProcessCommand { get; }
        public ICommand ExportPluginsCommand { get; }
        public ICommand ResetMessageCountsCommand { get; }
        public int ServicesCreated => Services.Count;
        public int CurrentActiveServices => Services.Count(s => s.IsActive);

        private bool _servicesRunning;
        public bool ServicesRunning
        {
            get => _servicesRunning;
            private set
            {
                if (_servicesRunning != value)
                {
                    _servicesRunning = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ServiceProcessButtonText));
                    RefreshServiceCommandStates();
                }
            }
        }

        private bool _isServiceProcessBusy;
        public bool IsServiceProcessBusy
        {
            get => _isServiceProcessBusy;
            private set
            {
                if (_isServiceProcessBusy != value)
                {
                    _isServiceProcessBusy = value;
                    OnPropertyChanged();
                    (ToggleServiceProcessCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                    RefreshServiceCommandStates();
                }
            }
        }

        public string ServiceProcessButtonText => ServicesRunning ? "Stop Services" : "Start Services";

        public ServiceLogViewModel LogViewModel { get; }

        public LogLevel LogLevelFilter
        {
            get => LogViewModel.LogLevelFilter;
            set => LogViewModel.LogLevelFilter = value;
        }

        public IEnumerable<LogEntry> DisplayLogs => LogViewModel.DisplayLogs;

        private void RefreshServiceCommandStates()
        {
            if (RemoveServiceCommand is AsyncRelayCommand<ServiceListModel?> removeCommand)
            {
                removeCommand.RaiseCanExecuteChanged();
            }

            if (EditServiceCommand is RelayCommand<ServiceListModel?> editCommand)
            {
                editCommand.RaiseCanExecuteChanged();
            }
        }

        private readonly CsvServiceAdapter _csvService;
        private readonly ILoggingService? _logger;
        private readonly IMessageRoutingService _messageRoutingService;
        private readonly INetworkConfigurationService _networkService;
        private readonly IServiceCatalog _serviceCatalog;
        private readonly IDictionary<ServiceType, IEditServiceHandler> _editHandlers;
        private readonly IStartupPreferencesService _startupPreferencesService;
        private readonly HashSet<ServiceListModel> _activatingServices = new();
        private readonly HashSet<ServiceListModel> _trackedServices = new();
        private readonly Dictionary<(ServiceType Type, string Name), ServiceListModel> _serviceIndex = new();
        private readonly Dictionary<string, HashSet<ServiceListModel>> _servicesByName = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<ServiceListModel, ServiceIndexEntry> _serviceKeys = new();
        private static readonly TimeSpan ActivationConfirmationDelay = TimeSpan.FromMilliseconds(500);
        private int _serviceCreationScopeDepth;

        private readonly record struct ServiceIndexEntry(ServiceType Type, string NormalizedName, string DisplayName);

        internal bool IsServiceCreationInProgress => Volatile.Read(ref _serviceCreationScopeDepth) > 0;

        internal int ActivatingServicesCount => _activatingServices.Count;

        public NetworkConfigurationViewModel NetworkConfig { get; }

        private bool CanModifyConfiguration => !IsServiceProcessBusy && !ServicesRunning;

        public MainViewModel(
            CsvServiceAdapter csvService,
            NetworkConfigurationViewModel networkConfig,
            INetworkConfigurationService networkService,
            IServiceCatalog serviceCatalog,
            IDictionary<ServiceType, IEditServiceHandler> editHandlers,
            IStartupPreferencesService startupPreferencesService,
            IMessageRoutingService messageRoutingService,
            ILoggingService? logger = null,
            IOptions<AppSettings>? appOptions = null,
            string? servicesFilePath = null)
        {
            _csvService = csvService;
            _networkService = networkService;
            _serviceCatalog = serviceCatalog ?? throw new ArgumentNullException(nameof(serviceCatalog));
            _messageRoutingService = messageRoutingService ?? throw new ArgumentNullException(nameof(messageRoutingService));
            _logger = logger;
            NetworkConfig = networkConfig;
            _editHandlers = editHandlers;
            _startupPreferencesService = startupPreferencesService ?? throw new ArgumentNullException(nameof(startupPreferencesService));
            var aggregatedLogCapacity = Math.Max(1, appOptions?.Value?.AggregatedLogRetention ?? DefaultAggregatedLogCapacity);
            AllLogs = new LimitedObservableCollection<LogEntry>(aggregatedLogCapacity);
            ServiceListModel.OptionsSerializerResolver = ResolveOptionsSerializer;
            ServiceListModel.CrossServiceAssociationsClearing += OnCrossServiceAssociationsClearing;
            ObserveTask(NetworkConfig.LoadAsync());
            _networkService.ConfigurationChanged += (_, cfg) => ApplyNetworkConfiguration(cfg);
            if (!string.IsNullOrWhiteSpace(servicesFilePath))
            {
                ServicePersistence.FilePath = servicesFilePath!;
                _logger?.Log($"Using service persistence path {ServicePersistence.FilePath}", LogLevel.Debug);
            }

            Services.CollectionChanged += OnServicesCollectionChanged;

            LogViewModel = new ServiceLogViewModel(ServiceType.Mqtt, AllLogs, null, isAggregated: true, newestFirstInSource: false);
            LogViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ServiceLogViewModel.DisplayLogs))
                    OnPropertyChanged(nameof(DisplayLogs));
                if (e.PropertyName == nameof(ServiceLogViewModel.LogLevelFilter))
                    OnPropertyChanged(nameof(LogLevelFilter));
            };

            AddServiceCommand = new RelayCommand(AddService);
            RemoveServiceCommand = new AsyncRelayCommand<ServiceListModel?>(RemoveServiceAsync, CanRemoveService);
            EditServiceCommand = new RelayCommand<ServiceListModel?>(EditService, svc => svc != null || ActiveService != null);
            ToggleServiceProcessCommand = new AsyncRelayCommand(ToggleServiceProcessAsync, () => !IsServiceProcessBusy);
            ExportPluginsCommand = new RelayCommand(OnExportPlugins);
            ResetMessageCountsCommand = new AsyncRelayCommand(ResetMessageCountsAsync);
            FilteredServices = CollectionViewSource.GetDefaultView(Services);
            Filters.PropertyChanged += (_, __) => ApplyFilters();
            foreach (var service in Services)
            {
                TrackService(service);
            }
            LogViewModel.UpdateServiceFilters(Services.Select(s => s.DisplayName));
            ServicesRunning = Services.Any(svc => svc.IsActive);
            ApplyFilters();
            if (_logger is LoggingService concreteLogger)
            {
                ObserveTask(concreteLogger.ReloadAsync());
            }
        }

        internal IDisposable BeginServiceCreationScope()
        {
            Interlocked.Increment(ref _serviceCreationScopeDepth);
            return new ServiceCreationScope(this);
        }

        private void EndServiceCreationScope()
        {
            var newDepth = Interlocked.Decrement(ref _serviceCreationScopeDepth);
            if (newDepth < 0)
            {
                Interlocked.Exchange(ref _serviceCreationScopeDepth, 0);
            }
        }

        private sealed class ServiceCreationScope : IDisposable
        {
            private MainViewModel? owner;

            public ServiceCreationScope(MainViewModel owner)
            {
                this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
            }

            public void Dispose()
            {
                owner?.EndServiceCreationScope();
                owner = null;
            }
        }

        private void ApplyNetworkConfiguration(NetworkConfiguration config)
        {
            var factory = App.UiThreadTaskFactory;
            if (factory is null)
            {
                ApplyNetworkConfigurationOnUiThread(config);
                return;
            }

            factory.Run(async () =>
            {
                await factory.SwitchToMainThreadAsync();
                ApplyNetworkConfigurationOnUiThread(config);
            });
        }

        private void ApplyNetworkConfigurationOnUiThread(NetworkConfiguration config)
        {
            foreach (var svc in Services)
            {
                if (svc.ServicePage?.DataContext is INetworkAwareViewModel navm)
                {
                    navm.UpdateNetworkConfiguration(config);
                }
            }
        }

        private void OnExportPlugins()
        {
            ExportPluginsRequested?.Invoke(this, EventArgs.Empty);
        }

        public event Action? AddServiceRequested;
        public event Action? ConfigurationChangeBlocked;
        public event EventHandler? ExportPluginsRequested;
        public event EventHandler<string>? HomeRequested;

        public bool RequestConfigurationChange()
        {
            if (!CanModifyConfiguration)
            {
                ConfigurationChangeBlocked?.Invoke();
                return false;
            }

            return true;
        }

        private bool CanRemoveService(ServiceListModel? service)
        {
            return (service ?? ActiveService) != null && CanModifyConfiguration;
        }

        private void EditService(ServiceListModel? service)
        {
            var target = service ?? ActiveService;
            if (target == null)
                return;

            if (!RequestConfigurationChange())
            {
                return;
            }

            if (_editHandlers.TryGetValue(target.Type, out var handler))
            {
                handler.Edit(target);
            }
            else
            {
                _logger?.Log($"No edit handler registered for {target.Type}", LogLevel.Warning);
            }
        }

        private void AddService()
        {
            if (!RequestConfigurationChange())
            {
                return;
            }

            _logger?.Log("AddService invoked", LogLevel.Debug);
            AddServiceRequested?.Invoke();
            _logger?.Log("AddService completed", LogLevel.Debug);
        }

        private async Task ResetMessageCountsAsync()
        {
            foreach (var service in Services)
            {
                service.ResetMessageCounts();
            }

            await SaveServicesAsync().ConfigureAwait(false);
            _logger?.Log("Message counters reset", LogLevel.Information);
        }

        internal string GenerateServiceName(ServiceType serviceType)
        {
            var typeName = serviceType.ToBaseName();
            int index = 1;
            foreach (var svc in Services.Where(s => s.Type == serviceType))
            {
                var namePart = svc.DisplayName;
                if (namePart.StartsWith(typeName, StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(namePart.Substring(typeName.Length), out int n) && n >= index)
                {
                    index = n + 1;
                }
            }
            return $"{typeName}{index}";
        }

        private async Task RemoveServiceAsync(ServiceListModel? service)
        {
            var target = service ?? ActiveService;
            if (target == null)
            {
                return;
            }

            if (!RequestConfigurationChange())
            {
                return;
            }

            _logger?.Log($"Removing service {target.DisplayName}", LogLevel.Debug);
            ClearRoutingCache(target.Type, target.DisplayName);
            target.ClearRoutingAttributes();
            var index = Services.IndexOf(target);
            target.AddLog("Service removed", WpfBrushes.Red);
            if (target.Type != ServiceType.Csv)
            {
                await _csvService.RemoveColumnsForServiceAsync(target.DisplayName);
            }

            RemoveServiceAssociations(target);
            _activatingServices.Remove(target);
            target.LogAdded -= OnServiceLogAdded;
            target.ActiveChanged -= OnServiceActiveChanged;
            Services.Remove(target);

            if (ReferenceEquals(ActiveService, target))
            {
                ActiveService = null;
            }

            if (Services.Count > 0)
            {
                if (index >= Services.Count)
                {
                    index = Services.Count - 1;
                }

                SelectedService = Services[index];
            }
            else
            {
                SelectedService = null;
            }

            OnPropertyChanged(nameof(ServicesCreated));
            OnPropertyChanged(nameof(CurrentActiveServices));
            LogViewModel.RefreshLogs();
            await SaveServicesAsync().ConfigureAwait(false);
            _logger?.Log("Service removed", LogLevel.Debug);
        }

        internal void ClearRoutingCache(ServiceType serviceType, string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return;
            }

            _messageRoutingService.ClearService(serviceType, serviceName);
            _messageRoutingService.ClearService(serviceName);
        }

        public async Task SaveServicesAsync()
        {
            // Update order prior to saving
            for (int i = 0; i < Services.Count; i++)
            {
                Services[i].Order = i;
            }
            foreach (var svc in Services)
            {
                if (svc.ServicePage?.DataContext is TcpServiceMessagesViewModel tcpVm)
                {
                    await tcpVm.PersistOptionsAsync().ConfigureAwait(false);
                }
            }
            await ServicePersistence.SaveAsync(Services, _serviceCatalog, _logger).ConfigureAwait(false);
        }

        public async Task LoadServicesAsync()
        {
            var existing = await ServicePersistence.LoadAsync(_serviceCatalog, _messageRoutingService, _logger).ConfigureAwait(true);
            foreach (var service in existing.OrderBy(s => s.Order))
            {
                if (string.IsNullOrWhiteSpace(service.DescriptorId))
                {
                    service.DescriptorId = ResolveDescriptorId(service.Type);
                }

                ClearRoutingCache(service.Type, service.DisplayName);
                var normalizedName = NormalizeDisplayName(service.Type, service.DisplayName);
                if (Services.Any(existingService => existingService.DisplayName.Equals(normalizedName, StringComparison.OrdinalIgnoreCase)))
                {
                    normalizedName = GenerateServiceName(service.Type);
                }

                ClearRoutingCache(service.Type, normalizedName);
                service.DisplayName = normalizedName;

                ApplyPresentationMetadata(service);
                service.LogAdded += OnServiceLogAdded;
                service.ActiveChanged += OnServiceActiveChanged;

                if (service.Type != ServiceType.Csv)
                {
                    _csvService.EnsureColumnsForService(service.DisplayName);
                }

                Services.Add(service);
                TrackService(service);

                service.RepublishRoutingAttributes();

                foreach (var log in service.LogState.Logs.Reverse())
                {
                    AllLogs.Add(log);
                }

                _logger?.Log($"Loaded service {service.DisplayName}", LogLevel.Debug);
            }
            OnPropertyChanged(nameof(ServicesCreated));
            OnPropertyChanged(nameof(CurrentActiveServices));
            LogViewModel.UpdateServiceFilters(Services.Select(s => s.DisplayName));
            ServicesRunning = Services.Any(svc => svc.IsActive);
            ApplyFilters();
            if (_logger is LoggingService concreteLogger)
            {
                await concreteLogger.ReloadAsync().ConfigureAwait(true);
            }
        }

        private IServiceOptionsSerializer? ResolveOptionsSerializer(string? descriptorId)
        {
            if (string.IsNullOrWhiteSpace(descriptorId))
            {
                return null;
            }

            return _serviceCatalog.TryGetById(descriptorId, out var descriptor)
                ? descriptor.OptionsSerializer
                : null;
        }

        private string? ResolveDescriptorId(ServiceType serviceType)
        {
            return _serviceCatalog.Descriptors.FirstOrDefault(d => d.ServiceType == serviceType)?.Id;
        }

        private void ApplyPresentationMetadata(ServiceListModel service)
        {
            if (service is null)
            {
                return;
            }

            var descriptorId = service.DescriptorId;
            if (string.IsNullOrWhiteSpace(descriptorId))
            {
                descriptorId = ResolveDescriptorId(service.Type);
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

        private static string NormalizeDisplayName(ServiceType type, string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return type.ToBaseName();
            }

            var trimmed = displayName.Trim();
            var separatorIndex = trimmed.LastIndexOf(" - ", StringComparison.Ordinal);
            if (separatorIndex >= 0)
            {
                trimmed = trimmed[(separatorIndex + 3)..];
            }

            var baseName = type.ToBaseName();

            var codePrefix = type.ToCode();
            if (!string.Equals(codePrefix, baseName, StringComparison.OrdinalIgnoreCase) &&
                trimmed.StartsWith(codePrefix, StringComparison.OrdinalIgnoreCase) &&
                !trimmed.StartsWith(baseName, StringComparison.OrdinalIgnoreCase))
            {
                trimmed = baseName + trimmed[codePrefix.Length..];
            }

            var enumName = type.ToString();
            if (!string.Equals(enumName, baseName, StringComparison.OrdinalIgnoreCase) &&
                trimmed.StartsWith(enumName, StringComparison.OrdinalIgnoreCase) &&
                !trimmed.StartsWith(baseName, StringComparison.OrdinalIgnoreCase))
            {
                trimmed = baseName + trimmed[enumName.Length..];
            }

            return trimmed;
        }

        private void ApplyFilters()
        {
            FilteredServices.Filter = obj =>
            {
                if (obj is not ServiceListModel svc)
                    return false;

                if (!string.IsNullOrWhiteSpace(Filters.NameFilter) &&
                    !svc.DisplayName.Contains(Filters.NameFilter, StringComparison.OrdinalIgnoreCase))
                    return false;

                if (Filters.TypeFilter != "All" &&
                    ServiceTypeExtensions.TryParse(Filters.TypeFilter, out var fType) &&
                    svc.Type != fType)
                    return false;

                if (Filters.StatusFilter == "Active" && !svc.IsActive)
                    return false;
                if (Filters.StatusFilter == "Inactive" && svc.IsActive)
                    return false;

                return true;
            };
            FilteredServices.Refresh();
        }

        private async Task ToggleServiceProcessAsync()
        {
            if (IsServiceCreationInProgress)
            {
                _logger?.Log("Service process toggle ignored because a service is being created.", LogLevel.Debug);
                return;
            }

            if (IsServiceProcessBusy)
            {
                return;
            }

            IsServiceProcessBusy = true;
            try
            {
                if (ServicesRunning)
                {
                    _logger?.Log("Stopping services", LogLevel.Information);
                    await StopServicesAsync();
                    ServicesRunning = false;
                }
                else
                {
                    if (Services.Count == 0)
                    {
                        _logger?.Log("No services configured to start.", LogLevel.Warning);
                        return;
                    }

                    var currentSettings = await UserSettingsStorage.LoadAsync(_logger);
                    var preferenceResult = _startupPreferencesService.ShowDialog(currentSettings.RunServicesOnStartup, currentSettings.RunUIOnStartup);
                    if (!preferenceResult.Accepted)
                    {
                        _logger?.Log("Start services cancelled from startup preferences dialog", LogLevel.Information);
                        return;
                    }

                    if (preferenceResult.RunServicesOnStartup != currentSettings.RunServicesOnStartup ||
                        preferenceResult.RunUIOnStartup != currentSettings.RunUIOnStartup)
                    {
                        currentSettings.RunServicesOnStartup = preferenceResult.RunServicesOnStartup;
                        currentSettings.RunUIOnStartup = preferenceResult.RunUIOnStartup;
                        await UserSettingsStorage.SaveAsync(currentSettings, _logger);
                    }

                    ServicesRunning = true;
                    _logger?.Log("Starting services", LogLevel.Information);
                    HomeRequested?.Invoke(this, "Start Services");
                    await StartServicesAsync();
                }
            }
            finally
            {
                IsServiceProcessBusy = false;
            }
        }

        public async Task ShutdownServicesAsync()
        {
            if (IsServiceCreationInProgress)
            {
                _logger?.Log("Shutdown skipped because a service is being created.", LogLevel.Debug);
                return;
            }

            if (!Services.Any(s => s.IsActive) && _activatingServices.Count == 0)
            {
                return;
            }

            while (IsServiceProcessBusy)
            {
                await Task.Delay(50);
            }

            IsServiceProcessBusy = true;
            try
            {
                await StopServicesAsync();
                ServicesRunning = false;
            }
            finally
            {
                IsServiceProcessBusy = false;
            }
        }

        private async Task StartServicesAsync()
        {
            if (IsServiceCreationInProgress)
            {
                _logger?.Log("Skipping service activation while creation is in progress.", LogLevel.Debug);
                return;
            }

            foreach (var svc in Services)
            {
                if (svc.IsActive)
                {
                    svc.SetRuntimeState(ServiceRuntimeState.Active);
                    continue;
                }

                if (!_activatingServices.Add(svc))
                {
                    continue;
                }

                svc.SetRuntimeState(ServiceRuntimeState.Activating);
                await Task.Yield();
                svc.IsActive = true;
                _ = CompleteActivationAsync(svc);
            }
        }

        private async Task StopServicesAsync()
        {
            if (IsServiceCreationInProgress)
            {
                _logger?.Log("Skipping service deactivation while creation is in progress.", LogLevel.Debug);
                return;
            }

            foreach (var svc in Services)
            {
                _activatingServices.Remove(svc);
                if (!svc.IsActive)
                {
                    svc.SetRuntimeState(ServiceRuntimeState.Inactive);
                    continue;
                }

                svc.IsActive = false;
                await Task.Yield();
            }
        }

        private async Task CompleteActivationAsync(ServiceListModel svc)
        {
            await Task.Delay(ActivationConfirmationDelay);
            if (_activatingServices.Contains(svc) && svc.RuntimeState != ServiceRuntimeState.Error)
            {
                svc.SetRuntimeState(ServiceRuntimeState.Active);
                _activatingServices.Remove(svc);
            }
        }

        public void OnServiceLogAdded(ServiceListModel svc, LogEntry entry)
        {
            if (svc is null || entry is null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(entry.ServiceName))
            {
                entry.ServiceName = svc.DisplayName;
            }

            entry.ServiceType ??= svc.Type;

            AllLogs.Add(entry);
            if (svc.Type != ServiceType.Csv && Services.Any(s => s.Type == ServiceType.Csv))
            {
                try
                {
                    _csvService.RecordLog(svc.DisplayName, entry.Message);
                }
                catch
                {
                    // ignore CSV errors during logging
                }
            }
            LogViewModel.RefreshLogs();

            if (entry.Level >= LogLevel.Error)
            {
                var wasActivating = _activatingServices.Remove(svc);
                if (svc.RuntimeState != ServiceRuntimeState.Error)
                {
                    svc.SetRuntimeState(ServiceRuntimeState.Error);
                }

                if (wasActivating)
                {
                    if (svc.IsActive)
                    {
                        svc.IsActive = false;
                    }

                    if (!Services.Any(s => s.IsActive) && _activatingServices.Count == 0)
                    {
                        ServicesRunning = false;
                    }
                }
            }
        }

        private void OnServicesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var tracked in _trackedServices.ToList())
                {
                    UntrackService(tracked);
                }

                foreach (var service in Services)
                {
                    TrackService(service);
                }
            }
            else
            {
                if (e.NewItems is not null)
                {
                    foreach (ServiceListModel service in e.NewItems)
                    {
                        TrackService(service);
                    }
                }

                if (e.OldItems is not null && e.Action != NotifyCollectionChangedAction.Move)
                {
                    foreach (ServiceListModel service in e.OldItems)
                    {
                        UntrackService(service);
                    }
                }
            }

            LogViewModel.UpdateServiceFilters(Services.Select(s => s.DisplayName));
        }

        private void TrackService(ServiceListModel service)
        {
            if (service is null)
            {
                return;
            }

            if (_trackedServices.Add(service))
            {
                AddServiceToIndex(service);
                service.PropertyChanged += OnServicePropertyChanged;
            }
        }

        private void UntrackService(ServiceListModel service)
        {
            if (service is null)
            {
                return;
            }

            if (_trackedServices.Remove(service))
            {
                service.PropertyChanged -= OnServicePropertyChanged;
                RemoveServiceFromIndex(service);
            }
        }

        private void AddServiceToIndex(ServiceListModel service)
        {
            var entry = CreateIndexEntry(service);
            _serviceIndex[(entry.Type, entry.NormalizedName)] = service;
            AddToNameIndex(service, entry.NormalizedName);
            _serviceKeys[service] = entry;
            service.ServiceLookup = this;
        }

        private void RemoveServiceFromIndex(ServiceListModel service)
        {
            if (_serviceKeys.Remove(service, out var entry))
            {
                _serviceIndex.Remove((entry.Type, entry.NormalizedName));
                RemoveFromNameIndex(service, entry.NormalizedName);
            }

            service.ServiceLookup = null;
        }

        private void UpdateServiceIndex(ServiceListModel service, bool updateAssociations)
        {
            if (!_serviceKeys.TryGetValue(service, out var previousEntry))
            {
                AddServiceToIndex(service);
                return;
            }

            var newEntry = CreateIndexEntry(service);
            var keyChanged = previousEntry.Type != newEntry.Type || previousEntry.NormalizedName != newEntry.NormalizedName;
            var nameChanged = !string.Equals(previousEntry.DisplayName, newEntry.DisplayName, StringComparison.Ordinal);

            if (!keyChanged)
            {
                if (nameChanged)
                {
                    _serviceKeys[service] = newEntry;
                    if (updateAssociations)
                    {
                        UpdateNeighborAssociations(service, previousEntry.DisplayName);
                    }
                }

                return;
            }

            _serviceIndex.Remove((previousEntry.Type, previousEntry.NormalizedName));
            RemoveFromNameIndex(service, previousEntry.NormalizedName);

            _serviceIndex[(newEntry.Type, newEntry.NormalizedName)] = service;
            AddToNameIndex(service, newEntry.NormalizedName);
            _serviceKeys[service] = newEntry;

            if (updateAssociations || nameChanged)
            {
                UpdateNeighborAssociations(service, previousEntry.DisplayName);
            }
        }

        private void AddToNameIndex(ServiceListModel service, string normalizedName)
        {
            if (!_servicesByName.TryGetValue(normalizedName, out var bucket))
            {
                bucket = new HashSet<ServiceListModel>();
                _servicesByName[normalizedName] = bucket;
            }

            bucket.Add(service);
        }

        private void RemoveFromNameIndex(ServiceListModel service, string normalizedName)
        {
            if (!_servicesByName.TryGetValue(normalizedName, out var bucket))
            {
                return;
            }

            bucket.Remove(service);
            if (bucket.Count == 0)
            {
                _servicesByName.Remove(normalizedName);
            }
        }

        private ServiceIndexEntry CreateIndexEntry(ServiceListModel service)
        {
            var displayName = service.DisplayName ?? string.Empty;
            return new ServiceIndexEntry(service.Type, NormalizeServiceName(displayName), displayName);
        }

        private static string NormalizeServiceName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            return name.Trim().ToUpperInvariant();
        }

        public bool TryGetService(ServiceType type, string name, out ServiceListModel? service)
        {
            return _serviceIndex.TryGetValue((type, NormalizeServiceName(name)), out service);
        }

        public IEnumerable<ServiceListModel> FindByDisplayName(string name)
        {
            var normalized = NormalizeServiceName(name);
            if (_servicesByName.TryGetValue(normalized, out var bucket) && bucket.Count > 0)
            {
                return bucket.ToArray();
            }

            return Array.Empty<ServiceListModel>();
        }

        private IReadOnlyList<ServiceListModel> GetAssociatedServiceNeighbors(ServiceListModel service)
        {
            if (service.AssociatedServices.Count == 0)
            {
                return Array.Empty<ServiceListModel>();
            }

            var neighbors = new HashSet<ServiceListModel>();
            var snapshot = service.AssociatedServices.ToList();
            foreach (var neighborName in snapshot)
            {
                foreach (var neighbor in FindByDisplayName(neighborName))
                {
                    if (!ReferenceEquals(neighbor, service))
                    {
                        neighbors.Add(neighbor);
                    }
                }
            }

            return neighbors.Count == 0 ? Array.Empty<ServiceListModel>() : neighbors.ToList();
        }

        private void UpdateNeighborAssociations(ServiceListModel service, string previousDisplayName)
        {
            if (service is null || string.IsNullOrWhiteSpace(previousDisplayName))
            {
                return;
            }

            foreach (var neighbor in GetAssociatedServiceNeighbors(service))
            {
                if (neighbor.AssociatedServices.Remove(previousDisplayName))
                {
                    var newName = service.DisplayName;
                    if (!string.IsNullOrWhiteSpace(newName) && !neighbor.AssociatedServices.Contains(newName))
                    {
                        neighbor.AssociatedServices.Add(newName);
                    }
                }
            }
        }

        private void ClearAssociationsFor(ServiceListModel service)
        {
            if (service is null)
            {
                return;
            }

            var namesToRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(service.DisplayName))
            {
                namesToRemove.Add(service.DisplayName);
            }

            if (service.AssociatedServices.Count == 0 && namesToRemove.Count == 0)
            {
                return;
            }

            foreach (var neighbor in GetAssociatedServiceNeighbors(service))
            {
                foreach (var name in namesToRemove)
                {
                    neighbor.AssociatedServices.Remove(name);
                }
            }

            service.AssociatedServices.Clear();
        }

        internal void RemoveServiceAssociations(ServiceListModel service)
        {
            ClearAssociationsFor(service);
        }

        private void OnCrossServiceAssociationsClearing()
        {
            foreach (var service in Services.ToList())
            {
                ClearAssociationsFor(service);
            }
        }

        private void OnServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ServiceListModel service)
            {
                return;
            }

            var propertyName = e.PropertyName;
            var affectsDisplayName = string.IsNullOrEmpty(propertyName) ||
                string.Equals(propertyName, nameof(ServiceListModel.DisplayName), StringComparison.Ordinal);
            var affectsIndex = affectsDisplayName ||
                string.Equals(propertyName, nameof(ServiceListModel.Type), StringComparison.Ordinal);

            if (affectsIndex)
            {
                UpdateServiceIndex(service, affectsDisplayName);
            }

            if (affectsDisplayName)
            {
                LogViewModel.UpdateServiceFilters(Services.Select(s => s.DisplayName));
            }
        }

        internal void OnServiceActiveChanged(bool _)
        {
            OnPropertyChanged(nameof(CurrentActiveServices));
            OnPropertyChanged(nameof(ServicesCreated));
        }

        public void ClearLogs()
        {
            LogViewModel.ClearLogs();
            _logger?.Log("Logs cleared", LogLevel.Debug);
        }

        public async Task ExportDisplayedLogsAsync(string filePath)
        {
            await LogViewModel.ExportLogsAsync(filePath);
            _logger?.Log($"Exported {LogViewModel.DisplayLogs.Count()} logs to {filePath}", LogLevel.Debug);
        }

        public async Task<(bool Success, string? ErrorMessage)> TryExportAllLogsAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _logger?.Log("Export aborted because the destination file path was empty.", LogLevel.Warning);
                return (false, "A valid file path was not provided.");
            }

            try
            {
                var lines = AllLogs.Reverse().Select(entry => entry.Message).ToList();
                await File.WriteAllLinesAsync(filePath, lines);
                _logger?.Log($"Exported {lines.Count} total logs to {filePath}", LogLevel.Information);
                return (true, null);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                _logger?.Log($"Failed to export logs to {filePath}: {ex.Message}", LogLevel.Error);
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                _logger?.Log($"Failed to export logs to {filePath}: {ex.Message}", LogLevel.Error);
                return (false, ex.Message);
            }
        }

        public void RefreshLogs()
        {
            LogViewModel.RefreshLogs();
            _logger?.Log("Logs refreshed", LogLevel.Debug);
        }

        private static void ObserveTask(Task? task)
        {
            if (task is null)
            {
                return;
            }

            _ = task.ContinueWith(
                t => _ = t.Exception,
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        // OnPropertyChanged inherited from ViewModelBase

    }

}
