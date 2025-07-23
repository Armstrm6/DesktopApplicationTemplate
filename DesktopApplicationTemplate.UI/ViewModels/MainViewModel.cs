using DesktopApplicationTemplate.UI.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using WpfBrush = System.Windows.Media.Brush;
using WpfBrushes = System.Windows.Media.Brushes;
using DesktopApplicationTemplate.UI.Services;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class LogEntry
    {
        public string Message { get; set; } = string.Empty;
        public WpfBrush Color { get; set; } = WpfBrushes.Black;
    }

    public class ServiceViewModel : INotifyPropertyChanged
    {
        public string DisplayName { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public Page? Page { get; set; }

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
                        AddLog("[Service Deactivated]", WpfBrushes.Red);                }
            }
        }

        public ObservableCollection<LogEntry> Logs { get; set; } = new();

        public event Action<ServiceViewModel, LogEntry>? LogAdded;
        public void AddLog(string message, WpfBrush? color = null)
        {
            string ts = DateTime.Now.ToString("MM.dd.yyyy - HH:mm:ss:ff");
            var entry = new LogEntry { Message = $"{ts} {message}", Color = color ?? WpfBrushes.Black };
            Logs.Insert(0, entry);
            LogAdded?.Invoke(this, entry);
        }


        public void AddLog(string message)
        {
            var timestamp = DateTime.Now.ToString("MM.dd.yyyy - HH:mm:ss.fffffff");
            var entry = new LogEntry
            {
                Message = $"{timestamp} {message}",
                Color = WpfBrushes.Black
            };
            Logs.Insert(0, entry);
            LogAdded?.Invoke(this, entry);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void SetColorsByType()
        {
            (BackgroundColor, BorderColor) = ServiceType switch
            {
                "TCP" => (WpfBrushes.LightBlue, WpfBrushes.DarkBlue),
                "HTTP" => (WpfBrushes.LightGreen, WpfBrushes.DarkGreen),
                "File Observer" => (WpfBrushes.LightSalmon, WpfBrushes.DarkSalmon),
                "HID" => (WpfBrushes.LightYellow, WpfBrushes.Goldenrod),
                "Heartbeat" => (WpfBrushes.LightPink, WpfBrushes.DeepPink),
                _ => (WpfBrushes.LightGray, WpfBrushes.Gray)
            };
            OnPropertyChanged(nameof(BackgroundColor));
            OnPropertyChanged(nameof(BorderColor));
        }
    }


    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ServiceViewModel> Services { get; set; } = new();
        public ICollectionView FilteredServices { get; }
        public FilterViewModel Filters { get; } = new();
        public ObservableCollection<LogEntry> AllLogs { get; } = new();
        private ServiceViewModel? _selectedService;
        public ServiceViewModel SelectedService
        {
            get => _selectedService!;
            set { _selectedService = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayLogs)); }
        }
        public ICommand AddServiceCommand { get; }
        public ICommand RemoveServiceCommand { get; }
        public event Action<ServiceViewModel>? EditRequested;
        public int ServicesCreated => Services.Count;
        public int CurrentActiveServices => Services.Count(s => s.IsActive);

        public IEnumerable<LogEntry> DisplayLogs => SelectedService?.Logs ?? AllLogs;

        public MainViewModel()
        {
            AddServiceCommand = new RelayCommand(AddService);
            RemoveServiceCommand = new RelayCommand(RemoveSelectedService, () => SelectedService != null);
            FilteredServices = CollectionViewSource.GetDefaultView(Services);
            Filters.PropertyChanged += (_, __) => ApplyFilters();
            LoadServices();
            ApplyFilters();
        }

        private void AddService()
        {
            var vm = new CreateServiceViewModel();
            var popup = new CreateServiceWindow(vm); // Replace with DI if needed
            if (popup.ShowDialog() == true)
            {
                var newService = new ServiceViewModel
                {
                    DisplayName = $"{popup.CreatedServiceType} - {popup.CreatedServiceName}",
                    ServiceType = popup.CreatedServiceType,
                    IsActive = false
                };
                newService.SetColorsByType();
                newService.LogAdded += OnServiceLogAdded;
                Services.Add(newService);
                OnPropertyChanged(nameof(ServicesCreated));
                OnPropertyChanged(nameof(CurrentActiveServices));
            }
        }

        private void RemoveSelectedService()
        {
            if (SelectedService != null)
            {
                var index = Services.IndexOf(SelectedService);
                SelectedService.LogAdded -= OnServiceLogAdded;
                Services.Remove(SelectedService);
                if (Services.Count > 0)
                {
                    if (index >= Services.Count) index = Services.Count - 1;
                    SelectedService = Services[index];
                }
                else
                {
                    SelectedService = null!;
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
            ServicePersistence.Save(Services);
        }

        private void LoadServices()
        {
            var existing = ServicePersistence.Load();
            foreach (var info in existing)
            {
                var svc = new ServiceViewModel
                {
                    DisplayName = info.DisplayName,
                    ServiceType = info.ServiceType,
                    IsActive = info.IsActive
                };
                svc.SetColorsByType();
                svc.LogAdded += OnServiceLogAdded;
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
            OnPropertyChanged(nameof(DisplayLogs));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
