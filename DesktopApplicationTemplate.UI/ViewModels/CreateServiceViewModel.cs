using System.Collections.ObjectModel;
using System.Collections.Generic;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.Core.Converters;

namespace DesktopApplicationTemplate.UI.ViewModels
{
    public class CreateServiceViewModel : ViewModelBase
    {
        public record ServiceTypeMetadata(ServiceType Type, string DisplayText, string Icon);

        public ObservableCollection<ServiceTypeMetadata> ServiceTypes { get; } = new()
        {
            new(ServiceType.Tcp, "TCP", "🔗"),
            new(ServiceType.Http, "HTTP", "🌐"),
            new(ServiceType.Csv, "CSV Creator", "📄"),
            new(ServiceType.Scp, "SCP", "📦"),
            new(ServiceType.Mqtt, "MQTT", "📡"),
            new(ServiceType.Ftp, "FTP Server", "🖥️")
        };

        private readonly HashSet<string> _existingNames;

        public CreateServiceViewModel(IEnumerable<string>? existingNames = null)
        {
            _existingNames = existingNames != null ? new HashSet<string>(existingNames) : new HashSet<string>();
        }

        public string GenerateDefaultName(ServiceType serviceType)
        {
            var typeName = ServiceTypeJsonConverter.ToLegacyString(serviceType);
            int index = 1;
            while (_existingNames.Contains($"{typeName}{index}"))
            {
                index++;
            }
            return $"{typeName}{index}";
        }
        // OnPropertyChanged provided by ViewModelBase
    }
}
