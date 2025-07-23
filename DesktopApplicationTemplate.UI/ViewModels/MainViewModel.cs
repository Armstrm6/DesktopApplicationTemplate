using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class LogEntry
    {
        public string Message { get; set; } = string.Empty;
        public Brush Color { get; set; } = Brushes.Black;
    }

    public class ServiceViewModel : INotifyPropertyChanged
    {
        public string DisplayName { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public Page? Page { get; set; }

        public Brush BackgroundColor { get; set; } = Brushes.LightGray;
        public Brush BorderColor { get; set; } = Brushes.Gray;
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
                        AddLog("[Service Activated]", Brushes.Green);
                    else
                        AddLog("[Service Deactivated]", Brushes.Red);                }
            }
        }

        public ObservableCollection<LogEntry> Logs { get; set; } = new();

        public event Action<ServiceViewModel, LogEntry>? LogAdded;
        public void AddLog(string message, Brush? color = null)
        {
            string ts = DateTime.Now.ToString("MM.dd.yyyy - HH:mm:ss:ff");
            var entry = new LogEntry { Message = $"{ts} {message}", Color = color ?? Brushes.Black };
            Logs.Insert(0, entry);
            LogAdded?.Invoke(this, entry);
        }


        public void AddLog(string message)
        {
            var timestamp = DateTime.Now.ToString("MM.dd.yyyy - HH:mm:ss.fffffff");
            var entry = new LogEntry
            {
                Message = $"{timestamp} {message}",
                Color = Brushes.Black
            };
            Logs.Insert(0, entry);
            LogAdded?.Invoke(this, entry);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void SetColorsByType()
        {
            (BackgroundColor, BorderColor) = ServiceType switch
            {
                "TCP" => (Brushes.LightBlue, Brushes.DarkBlue),
                "HTTP" => (Brushes.LightGreen, Brushes.DarkGreen),
                "File Observer" => (Brushes.LightSalmon, Brushes.DarkSalmon),
                "HID" => (Brushes.LightYellow, Brushes.Goldenrod),
                "Heartbeat" => (Brushes.LightPink, Brushes.DeepPink),
                _ => (Brushes.LightGray, Brushes.Gray)
            };
            OnPropertyChanged(nameof(BackgroundColor));
            OnPropertyChanged(nameof(BorderColor));
        }
    }


    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ServiceViewModel> Services { get; set; } = new();
        public ObservableCollection<LogEntry> AllLogs { get; } = new();
        private ServiceViewModel _selectedService;
        public ServiceViewModel SelectedService
        {
            get => _selectedService;
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
            LoadServices();
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
                SelectedService.LogAdded -= OnServiceLogAdded;
                Services.Remove(SelectedService);
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

        public void OnServiceLogAdded(ServiceViewModel svc, LogEntry entry)
        {
            AllLogs.Insert(0, entry);
            OnPropertyChanged(nameof(DisplayLogs));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
