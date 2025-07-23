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

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class ServiceViewModel : INotifyPropertyChanged
    {
        public string DisplayName { get; set; }

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
                        AddLog("[Service Activated]");
                    else
                        AddLog("[Service Deactivated]");
                }
            }
        }

        public ObservableCollection<string> Logs { get; set; } = new();

        public void AddLog(string message)
        {
            var timestamp = DateTime.Now.ToString("MM.dd.yyyy - HH:mm:ss.fffffff");
            Logs.Insert(0, $"{timestamp} {message}");
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
        public event Action<ServiceViewModel>? EditRequested;
        public int ServicesCreated => Services.Count;
        public int CurrentActiveServices => Services.Count(s => s.IsActive);

        public MainViewModel()
        {
            AddServiceCommand = new RelayCommand(AddService);
            RemoveServiceCommand = new RelayCommand(RemoveSelectedService, () => SelectedService != null);
        }

        private void AddService()
        {
            // Not used when using MainView UI navigation
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

        public void EditSelectedService()
        {
            if (SelectedService != null)
            {
                SelectedService.IsActive = false;
                EditRequested?.Invoke(SelectedService);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
