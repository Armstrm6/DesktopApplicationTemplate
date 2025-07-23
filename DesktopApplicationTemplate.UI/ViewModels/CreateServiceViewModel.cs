using System.Collections.ObjectModel;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class CreateServiceViewModel : ViewModelBase
    {
        public ObservableCollection<string> ServiceTypes { get; } = new()
        {
            "HID", "TCP", "HTTP", "File Observer", "Heartbeat", "CSV Creator",
            "SCP", "MQTT", "FTP"
        };

        private string _serviceName = string.Empty;
        public string ServiceName
        {
            get => _serviceName;
            set { _serviceName = value; OnPropertyChanged(); }
        }

        private string _selectedServiceType = string.Empty;
        public string SelectedServiceType
        {
            get => _selectedServiceType;
            set { _selectedServiceType = value; OnPropertyChanged(); }
        }

        // OnPropertyChanged provided by ViewModelBase
    }
}
