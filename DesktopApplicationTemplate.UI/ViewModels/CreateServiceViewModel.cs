using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class CreateServiceViewModel : ViewModelBase
    {
        public record ServiceTypeMetadata(string Type, string DisplayText, string Icon);

        public ObservableCollection<ServiceTypeMetadata> ServiceTypes { get; } = new()
        {
            new("TCP", "TCP", "🔗"),
            new("HTTP", "HTTP", "🌐"),
            new("CSV Creator", "CSV Creator", "📄"),
            new("SCP", "SCP", "📦"),
            new("MQTT", "MQTT", "📡"),
            new("FTP Server", "FTP Server", "🖥️")
        };

        private readonly HashSet<string> _existingNames;

        public CreateServiceViewModel(IEnumerable<string>? existingNames = null)
        {
            _existingNames = existingNames != null ? new HashSet<string>(existingNames) : new HashSet<string>();
        }

        public string GenerateDefaultName(string serviceType)
        {
            int index = 1;
            while (_existingNames.Contains($"{serviceType}{index}"))
            {
                index++;
            }
            return $"{serviceType}{index}";
        }
        // OnPropertyChanged provided by ViewModelBase
    }
}
