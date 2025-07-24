using DesktopApplicationTemplate.Models;
﻿using DesktopApplicationTemplate.UI.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using WpfBrush = System.Windows.Media.Brush;
using WpfBrushes = System.Windows.Media.Brushes;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels
{

    public class ServiceViewModel : ViewModelBase
    {
        public string DisplayName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public Page? Page { get; set; }
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
        public Page? ServicePage { get; set; }
 

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

        public void AddLog(string message, WpfBrush? color = null, LogLevel level = LogLevel.Debug)
        {
            var ts = DateTime.Now.ToString("MM.dd.yyyy - HH:mm:ss.fffffff");
            var entry = new LogEntry { Message = $"{ts} {message}", Color = color ?? WpfBrushes.Black, Level = level };
            Logs.Insert(0, entry);
            LogAdded?.Invoke(this, entry);
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

        public MainViewModel(CsvService csvService)
        {
            _csvService = csvService;
            AddServiceCommand = new RelayCommand(AddService);
            RemoveServiceCommand = new RelayCommand(RemoveSelectedService, () => SelectedService != null);
            FilteredServices = CollectionViewSource.GetDefaultView(Services);
            Filters.PropertyChanged += (_, __) => ApplyFilters();
            LoadServices();
            ApplyFilters();
        }

        private void AddService()
        {
            var existing = Services.Select(s => s.DisplayName.Split(" - ").Last());
            var vm = new CreateServiceViewModel(existing);
            var popup = new CreateServiceWindow(vm); // Replace with DI if needed
            if (popup.ShowDialog() == true)
            {
                string name = popup.CreatedServiceName;
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = GenerateServiceName(popup.CreatedServiceType);
                }
                var newService = new ServiceViewModel
                {
                    DisplayName = $"{popup.CreatedServiceType} - {name}",
                    ServiceType = popup.CreatedServiceType,
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
            }
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
                var index = Services.IndexOf(SelectedService);
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
            var existing = ServicePersistence.Load();
            foreach (var info in existing.OrderBy(i => i.Order))
            {
                var svc = new ServiceViewModel
                {
                    DisplayName = info.DisplayName,
                    ServiceType = info.ServiceType,
                    IsActive = info.IsActive,
                    Order = info.Order
                };
                svc.SetColorsByType();
                svc.LogAdded += OnServiceLogAdded;
                svc.ActiveChanged += OnServiceActiveChanged;
                Services.Add(svc);
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

        // OnPropertyChanged inherited from ViewModelBase
    }

}
