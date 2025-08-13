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

        private readonly HashSet<string> _existingNames;
        private bool _autoName = true;

        private string _serviceName = string.Empty;
        public string ServiceName
        {
            get => _serviceName;
            set { _serviceName = value; OnPropertyChanged(); _autoName = false; }
        }

        private string _selectedServiceType = string.Empty;
        public string SelectedServiceType
        {
            get => _selectedServiceType;
            set
            {
                _selectedServiceType = value;
                OnPropertyChanged();
                if (_autoName || string.IsNullOrWhiteSpace(ServiceName))
                    GenerateDefaultName();
            }
        }

        public CreateServiceViewModel(IEnumerable<string>? existingNames = null)
        {
            _existingNames = existingNames != null ? new HashSet<string>(existingNames) : new HashSet<string>();
            if (ServiceTypes.Count > 0)
            {
                _selectedServiceType = ServiceTypes[0];
                GenerateDefaultName();
            }
        }

        private void GenerateDefaultName()
        {
            int index = 1;
            while (_existingNames.Contains($"{SelectedServiceType}{index}"))
                index++;
            _autoName = true;
            _serviceName = $"{SelectedServiceType}{index}";
            OnPropertyChanged(nameof(ServiceName));
        }

        // OnPropertyChanged provided by ViewModelBase
    }
}
