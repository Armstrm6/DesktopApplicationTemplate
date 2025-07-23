using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Services
{
    public static class ServicePersistence
    {
        private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "services.json");

        public static void Save(IEnumerable<ServiceViewModel> services)
        {
            var data = new List<ServiceInfo>();
            foreach (var s in services)
            {
                data.Add(new ServiceInfo
                {
                    DisplayName = s.DisplayName,
                    ServiceType = s.ServiceType,
                    IsActive = s.IsActive,
                    Created = DateTime.Now
                });
            }
            File.WriteAllText(FilePath, JsonSerializer.Serialize(data));
        }

        public static List<ServiceInfo> Load()
        {
            if (!File.Exists(FilePath))
                return new List<ServiceInfo>();
            var json = File.ReadAllText(FilePath);
            try
            {
                return JsonSerializer.Deserialize<List<ServiceInfo>>(json) ?? new List<ServiceInfo>();
            }
            catch
            {
                return new List<ServiceInfo>();
            }
        }
    }

    public class ServiceInfo
    {
        public string DisplayName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
    }
}
