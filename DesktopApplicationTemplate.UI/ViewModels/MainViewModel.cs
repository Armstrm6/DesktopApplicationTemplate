using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApplicationTemplate.UI.Commands;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class ServiceViewModel
    {
        public string DisplayName { get; set; }
        public ObservableCollection<string> Logs { get; set; } = new();
        public bool IsActive { get; set; }
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

        public MainViewModel()
        {
            AddServiceCommand = new RelayCommand(AddService);
            RemoveServiceCommand = new RelayCommand(RemoveSelectedService, () => SelectedService != null);
        }

        private void AddService()
        {
            // Show selection dialog: HID / TCP / File / HTTPS
            var newService = new ServiceViewModel { DisplayName = "TCP/IP - New Service" };
            Services.Add(newService);
        }

        private void RemoveSelectedService()
        {
            if (SelectedService != null)
                Services.Remove(SelectedService);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
