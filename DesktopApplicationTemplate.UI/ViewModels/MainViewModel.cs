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
using DesktopApplicationTemplate.Core.Services.Protocols.Mqtt;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        public ObservableCollection<ServiceListModel> Services { get; set; } = new();
        public ICollectionView FilteredServices { get; }
        public FilterViewModel Filters { get; } = new();
        public ObservableCollection<LogEntry> AllLogs { get; } = new();
        private ServiceListModel? _selectedService;
        private ServiceListModel? _activeService;
        private bool _suppressActiveServiceReset;
        public ServiceListModel? SelectedService
        {
            get => _selectedService;
            set
            {
                _selectedService = value;
                OnPropertyChanged();
                if (!_suppressActiveServiceReset || value != null)
                {
                    ActiveService = value;
                }
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
                LogViewModel.SetLogs(_activeService?.Logs ?? AllLogs, _activeService is null);
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
        private static readonly TimeSpan ActivationConfirmationDelay = TimeSpan.FromMilliseconds(500);
        private int _serviceCreationScopeDepth;

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
            ServiceListModel.OptionsSerializerResolver = ResolveOptionsSerializer;
            _ = NetworkConfig.LoadAsync();
            _networkService.ConfigurationChanged += (_, cfg) => ApplyNetworkConfiguration(cfg);
            ServiceListModel.ResolveService = (type, name) =>
                Services.FirstOrDefault(s =>
                    s.Type == type &&
                    s.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(servicesFilePath))
            {
                ServicePersistence.FilePath = servicesFilePath!;
                _logger?.Log($"Using service persistence path {ServicePersistence.FilePath}", LogLevel.Debug);
            }

            Services.CollectionChanged += OnServicesCollectionChanged;

            LogViewModel = new ServiceLogViewModel(ServiceType.Mqtt, AllLogs, null, isAggregated: true);
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
            LoadServices();
            foreach (var service in Services)
            {
                TrackService(service);
            }
            LogViewModel.UpdateServiceFilters(Services.Select(s => s.DisplayName));
            ServicesRunning = Services.Any(svc => svc.IsActive);
            ApplyFilters();
            if (_logger is LoggingService concreteLogger)
            {
                concreteLogger.Reload();
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
                _csvService.RemoveColumnsForService(target.DisplayName);
            }

            ServiceListModel.RemoveServiceAssociations(target);
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
            ServicePersistence.Save(Services, _serviceCatalog, _logger);
        }

        private void LoadServices()
        {
            var existing = ServicePersistence.Load(_serviceCatalog, _messageRoutingService, _logger);
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

                service.RepublishRoutingAttributes();

                foreach (var log in service.Logs.Reverse())
                {
                    AllLogs.Insert(0, log);
                }

                _logger?.Log($"Loaded service {service.DisplayName}", LogLevel.Debug);
            }
            OnPropertyChanged(nameof(ServicesCreated));
            OnPropertyChanged(nameof(CurrentActiveServices));
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

                    var currentSettings = UserSettingsStorage.Load(_logger);
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
                        UserSettingsStorage.Save(currentSettings, _logger);
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

            AllLogs.Insert(0, entry);
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
            }
        }

        private void OnServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!string.Equals(e.PropertyName, nameof(ServiceListModel.DisplayName), StringComparison.Ordinal))
            {
                return;
            }

            LogViewModel.UpdateServiceFilters(Services.Select(s => s.DisplayName));
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

        public void ExportDisplayedLogs(string filePath)
        {
            LogViewModel.ExportLogs(filePath);
            _logger?.Log($"Exported {LogViewModel.DisplayLogs.Count()} logs to {filePath}", LogLevel.Debug);
        }

        public bool TryExportAllLogs(string filePath, out string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                errorMessage = "A valid file path was not provided.";
                _logger?.Log("Export aborted because the destination file path was empty.", LogLevel.Warning);
                return false;
            }

            try
            {
                var lines = AllLogs.Select(entry => entry.Message).ToList();
                File.WriteAllLines(filePath, lines);
                _logger?.Log($"Exported {lines.Count} total logs to {filePath}", LogLevel.Information);
                errorMessage = null;
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                errorMessage = ex.Message;
                _logger?.Log($"Failed to export logs to {filePath}: {ex.Message}", LogLevel.Error);
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                _logger?.Log($"Failed to export logs to {filePath}: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        public void RefreshLogs()
        {
            LogViewModel.RefreshLogs();
            _logger?.Log("Logs refreshed", LogLevel.Debug);
        }

        // OnPropertyChanged inherited from ViewModelBase

        internal IDisposable PreserveActiveServiceSelection()
        {
            return new ActiveServiceSelectionScope(this);
        }

        private sealed class ActiveServiceSelectionScope : IDisposable
        {
            private readonly MainViewModel _owner;
            private bool _disposed;

            public ActiveServiceSelectionScope(MainViewModel owner)
            {
                _owner = owner;
                _owner._suppressActiveServiceReset = true;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _owner._suppressActiveServiceReset = false;
                _disposed = true;
            }
        }
    }

}
