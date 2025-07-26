using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using DesktopApplicationTemplate.UI.ViewModels;

namespace DesktopApplicationTemplate.UI.Services
{
    public static class ServicePersistence
    {
        public static string FilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "services.json");

        public static void Save(IEnumerable<ServiceViewModel> services, ILoggingService? logger = null)
        {
            var data = new List<ServiceInfo>();
            var index = 0;
            foreach (var s in services)
            {
                data.Add(new ServiceInfo
                {
                    DisplayName = s.DisplayName,
                    ServiceType = s.ServiceType,
                    IsActive = s.IsActive,
                    Created = DateTime.Now,
                    Order = index++
                });
            }

            var json = JsonSerializer.Serialize(data);
            logger?.Log($"Persisting services to {FilePath}", LogLevel.Debug);
            using var fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var sw = new StreamWriter(fs);
            sw.Write(json);
            logger?.Log($"Saved {data.Count} services to {FilePath}", LogLevel.Debug);
        }

        public static List<ServiceInfo> Load(ILoggingService? logger = null)
        {
            if (!File.Exists(FilePath))
            {
                logger?.Log("Services file not found", LogLevel.Warning);
                return new List<ServiceInfo>();
            }
            var json = File.ReadAllText(FilePath);
            try
            {
                var result = JsonSerializer.Deserialize<List<ServiceInfo>>(json) ?? new List<ServiceInfo>();
                logger?.Log($"Loaded {result.Count} services", LogLevel.Debug);
                return result;
            }
            catch
            {
                logger?.Log("Failed to parse services file", LogLevel.Error);
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
        public int Order { get; set; }
    }
}
