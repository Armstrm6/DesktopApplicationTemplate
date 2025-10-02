using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.Service
{
    public class ServiceInfo
    {
        public string DisplayName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public int Order { get; set; }
    }

    public class ServiceManager : IDisposable
    {
        private readonly ILogger<ServiceManager> _logger;
        private readonly IConfiguration _config;
        private readonly string _filePath;
        private readonly Dictionary<string, RunningService> _running = new();
        private readonly Dictionary<string, ServiceRecord> _records = new();
        private readonly string _activeServicesFilePath;

        private sealed class RunningService
        {
            public CancellationTokenSource CancellationTokenSource { get; init; } = default!;
            public Task Task { get; init; } = default!;
            public DateTime StartTime { get; init; }
            public string ServiceType { get; init; } = string.Empty;
        }

        private sealed class ServiceRecord
        {
            public string Name { get; init; } = string.Empty;
            public string ServiceType { get; init; } = string.Empty;
            public DateTime StartTime { get; init; }
            public string Status { get; set; } = string.Empty;
        }

        public IReadOnlyCollection<string> ActiveServices => _running.Keys.ToList();

        public ServiceManager(ILogger<ServiceManager> logger, IConfiguration config, string? filePath = null)
        {
            _logger = logger;
            _config = config;
            _filePath = filePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "services.json");
            _activeServicesFilePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "activeservices.txt"));
        }

        public void Sync()
        {
            var infos = Load();
            var active = new HashSet<string>(infos.Where(i => i.IsActive).Select(i => i.DisplayName));

            foreach (var info in infos.Where(i => i.IsActive))
            {
                if (!_running.ContainsKey(info.DisplayName))
                    StartService(info);
            }

            foreach (var name in _running.Keys.ToList())
            {
                if (!active.Contains(name))
                    StopService(name);
            }
        }

        private List<ServiceInfo> Load()
        {
            if (!File.Exists(_filePath))
                return new List<ServiceInfo>();
            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<ServiceInfo>>(json) ?? new List<ServiceInfo>();
            }
            catch
            {
                return new List<ServiceInfo>();
            }
        }

        private void StartService(ServiceInfo info)
        {
            _logger.LogInformation("Starting {type} service: {name}", info.ServiceType, info.DisplayName);
            var cts = new CancellationTokenSource();
            var start = DateTime.UtcNow;
            var task = Task.Run(() => RunServiceLoop(info, cts.Token));
            _running[info.DisplayName] = new RunningService
            {
                CancellationTokenSource = cts,
                Task = task,
                StartTime = start,
                ServiceType = info.ServiceType
            };

            _records[info.DisplayName] = new ServiceRecord
            {
                Name = info.DisplayName,
                ServiceType = info.ServiceType,
                StartTime = start,
                Status = "Running"
            };

            WriteActiveServices();

            _logger.LogInformation("Started service: {name}", info.DisplayName);
        }

        private void StopService(string name)
        {
            if (_running.TryGetValue(name, out var service))
            {
                _logger.LogInformation("Stopping service: {name}", name);
                service.CancellationTokenSource.Cancel();
                _running.Remove(name);
                if (_records.TryGetValue(name, out var record))
                    record.Status = "Stopped";
                WriteActiveServices();
                _logger.LogInformation("Stopped service: {name}", name);
            }
        }

        private async Task RunServiceLoop(ServiceInfo info, CancellationToken token)
        {
            if (info.ServiceType == "Heartbeat")
            {
                var hb = _config.GetSection("Heartbeat");
                var message = hb.GetValue<string>("Message", "PING");
                var interval = hb.GetValue<int>("IntervalSeconds", 30);
                while (!token.IsCancellationRequested)
                {
                    _logger.LogInformation("[{name}] {msg}", info.DisplayName, message);
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(interval), token);
                    }
                    catch (TaskCanceledException)
                    {
                    }
                }
            }
            else
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10), token);
                    }
                    catch (TaskCanceledException)
                    {
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach (var running in _running.Values)
                running.CancellationTokenSource.Cancel();

            foreach (var record in _records.Values)
            {
                if (record.Status != "Stopped")
                    record.Status = "Stopped";
            }

            WriteActiveServices();

            _running.Clear();
            _records.Clear();
            if (File.Exists(_activeServicesFilePath))
            {
                try
                {
                    File.Delete(_activeServicesFilePath);
                }
                catch
                {
                }
            }
        }

        private void WriteActiveServices()
        {
            try
            {
                var pid = Process.GetCurrentProcess().Id;
                var lines = _records.Values
                    .Select(r => $"{pid}\t{r.Name}\t{r.ServiceType}\t{r.StartTime:o}\t{r.Status}")
                    .ToArray();
                File.WriteAllLines(_activeServicesFilePath, lines);
            }
            catch
            {
            }
        }
    }
}
