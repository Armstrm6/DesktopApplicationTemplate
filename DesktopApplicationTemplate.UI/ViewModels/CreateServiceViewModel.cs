using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class CreateServiceViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> ServiceTypes { get; } = new()
        {
            "HID", "TCP", "HTTP", "File Observer", "CSV Creator"
        };

        private string _serviceName;
        public string ServiceName
        {
            get => _serviceName;
            set { _serviceName = value; OnPropertyChanged(); }
        }

        private string _selectedServiceType;
        public string SelectedServiceType
        {
            get => _selectedServiceType;
            set { _selectedServiceType = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
