using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using DesktopApplicationTemplate.Models;

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
            var typeName = serviceType.ToLegacyString();
            int index = 1;
            foreach (var existing in _existingNames)
            {
                if (existing.StartsWith(typeName, StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(existing[typeName.Length..], out int value) && value >= index)
                {
                    index = value + 1;
                }
            }
            return $"{typeName}{index}";
        }

        public void SetExistingNames(IEnumerable<string> names)
        {
            _existingNames.Clear();
            foreach (var name in names)
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    _existingNames.Add(name);
                }
            }
        }
        // OnPropertyChanged provided by ViewModelBase
    }
}
