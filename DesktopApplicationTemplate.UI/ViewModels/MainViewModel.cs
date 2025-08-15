using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Views;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using System.Text.RegularExpressions;
using WpfBrush = System.Windows.Media.Brush;
using WpfBrushes = System.Windows.Media.Brushes;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.Core.Services;
using System.Text.Json.Serialization;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class ServiceViewModel : ViewModelBase
    {
        public string DisplayName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        [JsonIgnore] public Page? Page { get; set; }
        public int Order { get; set; }

        private WpfBrush _backgroundColor = WpfBrushes.LightGray;
        public WpfBrush BackgroundColor
        {
            get => _backgroundColor;
            set { _backgroundColor = value; OnPropertyChanged(); }
        }

        private WpfBrush _borderColor = WpfBrushes.Gray;
        public WpfBrush BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; OnPropertyChanged(); }
        }
        [JsonIgnore] public Page? ServicePage { get; set; }

        public ObservableCollection<string> AssociatedServices { get; } = new();

        public static Func<string, string, ServiceViewModel?>? ResolveService { get; set; }


        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                    if (_isActive)
                        AddLog("[Service Activated]", WpfBrushes.Green);
                    else
                        AddLog("[Service Deactivated]", WpfBrushes.Red);
                    ActiveChanged?.Invoke(_isActive);
                }
            }
        }

        public ObservableCollection<LogEntry> Logs { get; set; } = new();
        public event Action<bool>? ActiveChanged;

        public event Action<ServiceViewModel, LogEntry>? LogAdded;

        public void AddLog(string message, WpfBrush? color = null, LogLevel level = LogLevel.Debug, bool checkReference = true)
        {
            var ts = DateTime.Now.ToString("MM.dd.yyyy - HH:mm:ss.fffffff");
            var entry = new LogEntry { Message = $"{ts} {message}", Color = color ?? WpfBrushes.Black, Level = level };
            Logs.Insert(0, entry);
            LogAdded?.Invoke(this, entry);
            if (checkReference)
            {
                HandleReference(message, color ?? WpfBrushes.Black, level);
            }
        }

        private void HandleReference(string message, WpfBrush color, LogLevel level)
        {
            var m = Regex.Match(message, @"^([^.]+)\.([^.]+)\.(.+)$");
            if (m.Success && ResolveService != null)
            {
                var type = m.Groups[1].Value;
                var name = m.Groups[2].Value;
                var msg = m.Groups[3].Value;
                var target = ResolveService(type, name);
                if (target != null && target != this)
                {
                    if (!AssociatedServices.Contains(target.DisplayName))
                        AssociatedServices.Add(target.DisplayName);
                    if (!target.AssociatedServices.Contains(DisplayName))
                        target.AssociatedServices.Add(DisplayName);
                    target.AddLog(msg, color, level, false);
                }
            }
        }

        public void SetColorsByType()
        {
            (BackgroundColor, BorderColor) = ServiceType switch
            {
                "TCP" => (WpfBrushes.LightBlue, WpfBrushes.DarkBlue),
                "HTTP" => (WpfBrushes.LightGreen, WpfBrushes.DarkGreen),
                "File Observer" => (WpfBrushes.LightSalmon, WpfBrushes.DarkSalmon),
                "HID" => (WpfBrushes.LightYellow, WpfBrushes.Goldenrod),
                "Heartbeat" => (WpfBrushes.LightPink, WpfBrushes.DeepPink),
                "SCP" => (WpfBrushes.LightCyan, WpfBrushes.CadetBlue),
                "MQTT" => (WpfBrushes.LightGoldenrodYellow, WpfBrushes.Goldenrod),
                "FTP" => (WpfBrushes.LightSteelBlue, WpfBrushes.SteelBlue),
                _ => (WpfBrushes.LightGray, WpfBrushes.Gray)
            };
            OnPropertyChanged(nameof(BackgroundColor));
            OnPropertyChanged(nameof(BorderColor));
        }
    }


    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<ServiceViewModel> Services { get; set; } = new();
        public ICollectionView FilteredServices { get; }
        public FilterViewModel Filters { get; } = new();
        public ObservableCollection<LogEntry> AllLogs { get; } = new();
        private ServiceViewModel? _selectedService;
        public ServiceViewModel? SelectedService
        {
            get => _selectedService;
            set { _selectedService = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayLogs)); }
        }
        public ICommand AddServiceCommand { get; }
        public ICommand RemoveServiceCommand { get; }
        public event Action<ServiceViewModel>? EditRequested;
        public int ServicesCreated => Services.Count;
        public int CurrentActiveServices => Services.Count(s => s.IsActive);

        private LogLevel _logLevelFilter = LogLevel.Debug;
        public LogLevel LogLevelFilter
        {
            get => _logLevelFilter;
            set { _logLevelFilter = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayLogs)); }
        }

        public IEnumerable<LogEntry> DisplayLogs => (SelectedService?.Logs ?? AllLogs).Where(l => l.Level >= LogLevelFilter);

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
            ServiceViewModel.ResolveService = (type, name) =>
                Services.FirstOrDefault(s =>
                    s.ServiceType.Equals(type, StringComparison.OrdinalIgnoreCase) &&
                    s.DisplayName.Split(" - ").Last().Equals(name, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(servicesFilePath))
            {
                ServicePersistence.FilePath = servicesFilePath!;
                _logger?.Log($"Using service persistence path {ServicePersistence.FilePath}", LogLevel.Debug);
            }
            AddServiceCommand = new RelayCommand(AddService);
            RemoveServiceCommand = new RelayCommand(RemoveSelectedService, () => SelectedService != null);
            FilteredServices = CollectionViewSource.GetDefaultView(Services);
            Filters.PropertyChanged += (_, __) => ApplyFilters();
            LoadServices();
            ApplyFilters();
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

        private void AddService()
        {
            _logger?.Log("AddService invoked", LogLevel.Debug);
            var existing = Services.Select(s => s.DisplayName.Split(" - ").Last());
            var vm = new CreateServiceViewModel(existing);
            var popup = new CreateServiceWindow(vm); // Replace with DI if needed
            if (popup.ShowDialog() == true)
            {
                var name = popup.CreatedServiceName;
                var type = popup.CreatedServiceType;
                if (Services.Any(s => s.DisplayName.Split(" - ").Last().Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    name = GenerateServiceName(type);
                }
                var newService = new ServiceViewModel
                {
                    DisplayName = $"{type} - {name}",
                    ServiceType = type,
                    IsActive = false,
                    Order = Services.Count
                };
                newService.SetColorsByType();
                newService.LogAdded += OnServiceLogAdded;
                newService.ActiveChanged += OnServiceActiveChanged;
                newService.AddLog($"Default name '{name}' assigned", WpfBrushes.Gray);
                newService.AddLog("Service created", WpfBrushes.Blue);
                _csvService.EnsureColumnsForService(newService.DisplayName);
                Services.Add(newService);
                OnPropertyChanged(nameof(ServicesCreated));
                OnPropertyChanged(nameof(CurrentActiveServices));
                _logger?.Log($"Service {newService.DisplayName} created", LogLevel.Debug);
            }
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

        private void RemoveSelectedService()
        {
            if (SelectedService != null)
            {
                _logger?.Log($"Removing service {SelectedService.DisplayName}", LogLevel.Debug);
                var index = Services.IndexOf(SelectedService);
                SelectedService.AddLog("Service removed", WpfBrushes.Red);
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
                OnPropertyChanged(nameof(DisplayLogs));
                SaveServices();
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

        public void SaveServices()
        {
            // Update order prior to saving
            for (int i = 0; i < Services.Count; i++)
            {
                Services[i].Order = i;
            }
            ServicePersistence.Save(Services);
        }

        private void LoadServices()
        {
            var existing = ServicePersistence.Load(_logger);
            foreach (var info in existing.OrderBy(i => i.Order))
            {
                var svc = new ServiceViewModel
                {
                    DisplayName = info.DisplayName,
                    ServiceType = info.ServiceType,
                    IsActive = info.IsActive,
                    Order = info.Order
                };
                foreach (var a in info.AssociatedServices ?? new List<string>())
                    svc.AssociatedServices.Add(a);
                svc.SetColorsByType();
                svc.LogAdded += OnServiceLogAdded;
                svc.ActiveChanged += OnServiceActiveChanged;
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
                if (obj is not ServiceViewModel svc)
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

        public void OnServiceLogAdded(ServiceViewModel svc, LogEntry entry)
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
            OnPropertyChanged(nameof(DisplayLogs));
        }

        internal void OnServiceActiveChanged(bool _)
        {
            OnPropertyChanged(nameof(CurrentActiveServices));
        }

        public void ClearLogs()
        {
            if (SelectedService != null)
            {
                SelectedService.Logs.Clear();
            }
            else
            {
                AllLogs.Clear();
            }
            OnPropertyChanged(nameof(DisplayLogs));
            _logger?.Log("Logs cleared", LogLevel.Debug);
        }

        public void ExportDisplayedLogs(string filePath)
        {
            var lines = DisplayLogs.Select(l => l.Message).ToList();
            File.WriteAllLines(filePath, lines);
            _logger?.Log($"Exported {lines.Count} logs to {filePath}", LogLevel.Debug);
        }

        public void RefreshLogs()
        {
            OnPropertyChanged(nameof(DisplayLogs));
            _logger?.Log("Logs refreshed", LogLevel.Debug);
        }

        // OnPropertyChanged inherited from ViewModelBase
    }

}
