using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class CreateServiceViewModel : ViewModelBase
    {
        public record ServiceTypeMetadata(string Type, string DisplayText, string IconPath);

        public ObservableCollection<ServiceTypeMetadata> ServiceTypes { get; } = new()
        {
            new("HID", "HID", "/Assets/DesktopTemplateLayout-Logo.png"),
            new("TCP", "TCP", "/Assets/DesktopTemplateLayout-Logo.png"),
            new("HTTP", "HTTP", "/Assets/DesktopTemplateLayout-Logo.png"),
            new("File Observer", "File Observer", "/Assets/DesktopTemplateLayout-Logo.png"),
            new("Heartbeat", "Heartbeat", "/Assets/DesktopTemplateLayout-Logo.png"),
            new("CSV Creator", "CSV Creator", "/Assets/DesktopTemplateLayout-Logo.png"),
            new("SCP", "SCP", "/Assets/DesktopTemplateLayout-Logo.png"),
            new("MQTT", "MQTT", "/Assets/DesktopTemplateLayout-Logo.png"),
            new("FTP", "FTP", "/Assets/DesktopTemplateLayout-Logo.png")
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
