using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using WpfBrushes = System.Windows.Media.Brushes;
using DesktopApplicationTemplate.Core.Models;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Persistence;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels.Tcp;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Helpers;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<ServiceListModel> Services { get; set; } = new();
        public ICollectionView FilteredServices { get; }
        public FilterViewModel Filters { get; } = new();
        public ObservableCollection<LogEntry> AllLogs { get; } = new();
        private ServiceListModel? _selectedService;
        public ServiceListModel? SelectedService
        {
            get => _selectedService;
            set
            {
                _selectedService = value;
                OnPropertyChanged();
                LogViewModel.SetLogs(_selectedService?.Logs ?? AllLogs);
                (RemoveServiceCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (EditServiceCommand as RelayCommand<ServiceListModel?>)?.RaiseCanExecuteChanged();
            }
        }
        public ICommand AddServiceCommand { get; }
        public ICommand RemoveServiceCommand { get; }
        public ICommand EditServiceCommand { get; }
        public ICommand ToggleServiceProcessCommand { get; }
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

        private readonly CsvServiceAdapter _csvService;
        private readonly ILoggingService? _logger;
        private readonly INetworkConfigurationService _networkService;
        private readonly IDictionary<ServiceType, IEditServiceHandler> _editHandlers;
        private readonly HashSet<ServiceListModel> _activatingServices = new();
        private static readonly TimeSpan ActivationConfirmationDelay = TimeSpan.FromMilliseconds(500);

        public NetworkConfigurationViewModel NetworkConfig { get; }

        public MainViewModel(CsvServiceAdapter csvService, NetworkConfigurationViewModel networkConfig, INetworkConfigurationService networkService, IDictionary<ServiceType, IEditServiceHandler> editHandlers, ILoggingService? logger = null, string? servicesFilePath = null)
        {
            _csvService = csvService;
            _networkService = networkService;
            _logger = logger;
            NetworkConfig = networkConfig;
            _editHandlers = editHandlers;
            _ = NetworkConfig.LoadAsync();
            _networkService.ConfigurationChanged += (_, cfg) => ApplyNetworkConfiguration(cfg);
            ServiceListModel.ResolveService = (type, name) =>
                Services.FirstOrDefault(s =>
                    s.Type == type &&
                    s.DisplayName.Split(" - ").Last().Equals(name, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(servicesFilePath))
            {
                ServicePersistence.FilePath = servicesFilePath!;
                _logger?.Log($"Using service persistence path {ServicePersistence.FilePath}", LogLevel.Debug);
            }
            AddServiceCommand = new RelayCommand(AddService);
            RemoveServiceCommand = new AsyncRelayCommand(RemoveSelectedServiceAsync, () => SelectedService != null);
            EditServiceCommand = new RelayCommand<ServiceListModel?>(EditService, svc => svc != null);
            ToggleServiceProcessCommand = new AsyncRelayCommand(ToggleServiceProcessAsync, () => !IsServiceProcessBusy);
            FilteredServices = CollectionViewSource.GetDefaultView(Services);
            Filters.PropertyChanged += (_, __) => ApplyFilters();
            LoadServices();
            ServicesRunning = Services.Any(svc => svc.IsActive);
            ApplyFilters();
            LogViewModel = new ServiceLogViewModel(ServiceType.Mqtt, AllLogs);
            LogViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ServiceLogViewModel.DisplayLogs))
                    OnPropertyChanged(nameof(DisplayLogs));
                if (e.PropertyName == nameof(ServiceLogViewModel.LogLevelFilter))
                    OnPropertyChanged(nameof(LogLevelFilter));
            };
            if (_logger is LoggingService concreteLogger)
            {
                concreteLogger.Reload();
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

        public event Action? AddServiceRequested;
        public event Action? ConfigurationChangeBlocked;

        public bool RequestConfigurationChange()
        {
            if (IsServiceProcessBusy || ServicesRunning)
            {
                ConfigurationChangeBlocked?.Invoke();
                return false;
            }

            return true;
        }

        private void EditService(ServiceListModel? service)
        {
            var target = service ?? SelectedService;
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

        internal string GenerateServiceName(ServiceType serviceType)
        {
            var typeName = serviceType.ToLegacyString();
            int index = 1;
            foreach (var svc in Services.Where(s => s.Type == serviceType))
            {
                var namePart = svc.DisplayName.Split(" - ").Last();
                if (namePart.StartsWith(typeName) &&
                    int.TryParse(namePart.Substring(typeName.Length), out int n) && n >= index)
                {
                    index = n + 1;
                }
            }
            return $"{typeName}{index}";
        }

        private async Task RemoveSelectedServiceAsync()
        {
            if (SelectedService == null)
            {
                return;
            }

            if (!RequestConfigurationChange())
            {
                return;
            }

            _logger?.Log($"Removing service {SelectedService.DisplayName}", LogLevel.Debug);
            var index = Services.IndexOf(SelectedService);
            SelectedService.AddLog("Service removed", WpfBrushes.Red);
            if (SelectedService.Type != ServiceType.Csv)
                _csvService.RemoveColumnsForService(SelectedService.DisplayName);
            _activatingServices.Remove(SelectedService);
            SelectedService.LogAdded -= OnServiceLogAdded;
            SelectedService.ActiveChanged -= OnServiceActiveChanged;
            Services.Remove(SelectedService);
            if (Services.Count > 0)
            {
                if (index >= Services.Count) index = Services.Count - 1;
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
                    await tcpVm.SaveAsync().ConfigureAwait(false);
                }
            }
            ServicePersistence.Save(Services);
        }

        private void LoadServices()
        {
            var existing = ServicePersistence.Load(_logger);
            foreach (var info in existing.OrderBy(i => i.Order))
            {
                var svc = new ServiceListModel
                {
                    DisplayName = info.DisplayName,
                    Type = info.ServiceType,
                    IsActive = info.IsActive,
                    Order = info.Order,
                    TcpOptions = info.TcpOptions,
                    FtpOptions = info.FtpOptions,
                    HttpOptions = info.HttpOptions,
                    CsvOptions = info.CsvOptions,
                    HeartbeatOptions = info.HeartbeatOptions,
                    FileObserverOptions = info.FileObserverOptions,
                    HidOptions = info.HidOptions,
                    ScpOptions = info.ScpOptions,
                    TotalExecutionTimeMs = info.TotalExecutionTimeMs,
                    ExecutionCount = info.ExecutionCount
                };
                foreach (var a in info.AssociatedServices ?? new List<string>())
                    svc.AssociatedServices.Add(a);
                svc.SetColorsByType();
                svc.SetRuntimeState(svc.IsActive ? ServiceRuntimeState.Active : ServiceRuntimeState.Inactive);
                svc.LogAdded += OnServiceLogAdded;
                svc.ActiveChanged += OnServiceActiveChanged;
                if (svc.Type != ServiceType.Csv)
                    _csvService.EnsureColumnsForService(svc.DisplayName);
                Services.Add(svc);
                _logger?.Log($"Loaded service {svc.DisplayName}", LogLevel.Debug);
            }
            OnPropertyChanged(nameof(ServicesCreated));
            OnPropertyChanged(nameof(CurrentActiveServices));
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

                    ServicesRunning = true;
                    _logger?.Log("Starting services", LogLevel.Information);
                    await StartServicesAsync();
                }
            }
            finally
            {
                IsServiceProcessBusy = false;
            }
        }

        private async Task StartServicesAsync()
        {
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

            if (_activatingServices.Contains(svc) && entry.Level >= LogLevel.Error)
            {
                _activatingServices.Remove(svc);
                svc.SetRuntimeState(ServiceRuntimeState.Error);
                svc.IsActive = false;
                if (!Services.Any(s => s.IsActive) && _activatingServices.Count == 0)
                {
                    ServicesRunning = false;
                }
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

        public void ExportDisplayedLogs(string filePath)
        {
            LogViewModel.ExportLogs(filePath);
            _logger?.Log($"Exported {LogViewModel.DisplayLogs.Count()} logs to {filePath}", LogLevel.Debug);
        }

        public void RefreshLogs()
        {
            LogViewModel.RefreshLogs();
            _logger?.Log("Logs refreshed", LogLevel.Debug);
        }

        // OnPropertyChanged inherited from ViewModelBase
    }

}
