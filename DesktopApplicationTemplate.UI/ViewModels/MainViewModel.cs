using DesktopApplicationTemplate.UI.Views;
using DesktopApplicationTemplate.UI.ViewModels;
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
        public Page? Page { get; set; }

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
                        AddLog("[Service Deactivated]", Brushes.Red);
                }
            }
        }

        public ObservableCollection<LogEntry> Logs { get; set; } = new();
        public void AddLog(string message, Brush? color = null)
        {
            string ts = DateTime.Now.ToString("MM.dd.yyyy - HH:mm:ss:ff");
            Logs.Insert(0, new LogEntry { Message = $"{ts} {message}", Color = color ?? Brushes.Black });
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }


    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ServiceViewModel> Services { get; set; } = new();
        private ServiceViewModel _selectedService;
        public ServiceViewModel SelectedService
        {
            get => _selectedService;
            set { _selectedService = value; OnPropertyChanged(); }
        }
        public ICommand AddServiceCommand { get; }
        public ICommand RemoveServiceCommand { get; }
        public int ServicesCreated => Services.Count;
        public int CurrentActiveServices => Services.Count(s => s.IsActive);

        public MainViewModel()
        {
            AddServiceCommand = new RelayCommand(AddService);
            RemoveServiceCommand = new RelayCommand(RemoveSelectedService, () => SelectedService != null);
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
                    IsActive = false
                };
                Services.Add(newService);
                OnPropertyChanged(nameof(ServicesCreated));
                OnPropertyChanged(nameof(CurrentActiveServices));
            }
        }

        private void RemoveSelectedService()
        {
            if (SelectedService != null)
            {
                Services.Remove(SelectedService);
                OnPropertyChanged(nameof(ServicesCreated));
                OnPropertyChanged(nameof(CurrentActiveServices));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
