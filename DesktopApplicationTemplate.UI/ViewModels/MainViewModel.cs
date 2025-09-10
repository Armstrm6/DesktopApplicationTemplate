using DesktopApplicationTemplate.Models;
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
using DesktopApplicationTemplate.UI.Services;
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
        public event Action<ServiceListModel>? EditRequested;
        public int ServicesCreated => Services.Count;
        public int CurrentActiveServices => Services.Count(s => s.IsActive);

        public ServiceLogViewModel LogViewModel { get; }

        public LogLevel LogLevelFilter
        {
            get => LogViewModel.LogLevelFilter;
            set => LogViewModel.LogLevelFilter = value;
        }

        public IEnumerable<LogEntry> DisplayLogs => LogViewModel.DisplayLogs;

        private readonly CsvService _csvService;
        private readonly ILoggingService? _logger;
        private readonly INetworkConfigurationService _networkService;

        public NetworkConfigurationViewModel NetworkConfig { get; }

        public MainViewModel(CsvService csvService, NetworkConfigurationViewModel networkConfig, INetworkConfigurationService networkService, ILoggingService? logger = null, string? servicesFilePath = null)
        {
            _csvService = csvService;
            _networkService = networkService;
            _logger = logger;
            NetworkConfig = networkConfig;
            _ = NetworkConfig.LoadAsync();
            _networkService.ConfigurationChanged += (_, cfg) => ApplyNetworkConfiguration(cfg);
            ServiceListModel.ResolveService = (type, name) =>
                Services.FirstOrDefault(s =>
                    s.ServiceType.Equals(type, StringComparison.OrdinalIgnoreCase) &&
                    s.DisplayName.Split(" - ").Last().Equals(name, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(servicesFilePath))
            {
                ServicePersistence.FilePath = servicesFilePath!;
                _logger?.Log($"Using service persistence path {ServicePersistence.FilePath}", LogLevel.Debug);
            }
            AddServiceCommand = new RelayCommand(AddService);
            RemoveServiceCommand = new AsyncRelayCommand(RemoveSelectedServiceAsync, () => SelectedService != null);
            EditServiceCommand = new RelayCommand<ServiceListModel?>(EditService, svc => svc != null);
            FilteredServices = CollectionViewSource.GetDefaultView(Services);
            Filters.PropertyChanged += (_, __) => ApplyFilters();
            LoadServices();
            ApplyFilters();
            LogViewModel = new ServiceLogViewModel("Main", "Main", AllLogs);
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
        private void EditService(ServiceListModel? service)
        {
            var target = service ?? SelectedService;
            if (target != null)
            {
                EditRequested?.Invoke(target);
            }
        }

        private void AddService()
        {
            _logger?.Log("AddService invoked", LogLevel.Debug);
            AddServiceRequested?.Invoke();
            _logger?.Log("AddService completed", LogLevel.Debug);
        }

        internal string GenerateServiceName(string serviceType)
        {
            int index = 1;
            foreach (var svc in Services.Where(s => s.ServiceType == serviceType))
            {
                var namePart = svc.DisplayName.Split(" - ").Last();
                if (namePart.StartsWith(serviceType) &&
                    int.TryParse(namePart.Substring(serviceType.Length), out int n) && n >= index)
                {
                    index = n + 1;
                }
            }
            return $"{serviceType}{index}";
        }

        private async Task RemoveSelectedServiceAsync()
        {
            if (SelectedService != null)
            {
                _logger?.Log($"Removing service {SelectedService.DisplayName}", LogLevel.Debug);
                var index = Services.IndexOf(SelectedService);
                SelectedService.AddLog("Service removed", WpfBrushes.Red);
                if (!SelectedService.ServiceType.Contains("CSV", StringComparison.OrdinalIgnoreCase))
                    _csvService.RemoveColumnsForService(SelectedService.DisplayName);
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
        }

        public void EditSelectedService()
        {
            if (SelectedService != null)
            {
                SelectedService.IsActive = false;
                EditRequested?.Invoke(SelectedService);
            }
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
                    ServiceType = info.ServiceType,
                    IsActive = info.IsActive,
                    Order = info.Order,
                    TcpOptions = info.TcpOptions,
                    FtpOptions = info.FtpOptions,
                    HttpOptions = info.HttpOptions,
                    CsvOptions = info.CsvOptions,
                    TotalExecutionTimeMs = info.TotalExecutionTimeMs,
                    ExecutionCount = info.ExecutionCount
                };
                foreach (var a in info.AssociatedServices ?? new List<string>())
                    svc.AssociatedServices.Add(a);
                svc.SetColorsByType();
                svc.LogAdded += OnServiceLogAdded;
                svc.ActiveChanged += OnServiceActiveChanged;
                if (!svc.ServiceType.Contains("CSV", StringComparison.OrdinalIgnoreCase))
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

                if (Filters.TypeFilter != "All" && svc.ServiceType != Filters.TypeFilter)
                    return false;

                if (Filters.StatusFilter == "Active" && !svc.IsActive)
                    return false;
                if (Filters.StatusFilter == "Inactive" && svc.IsActive)
                    return false;

                return true;
            };
            FilteredServices.Refresh();
        }

        public void OnServiceLogAdded(ServiceListModel svc, LogEntry entry)
        {
            AllLogs.Insert(0, entry);
            if (svc.ServiceType != "CSV Creator" && Services.Any(s => s.ServiceType == "CSV Creator"))
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
