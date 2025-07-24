using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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
    }

    public class ServiceManager : IDisposable
    {
        private readonly ILogger<ServiceManager> _logger;
        private readonly IConfiguration _config;
        private readonly string _filePath;
        private readonly Dictionary<string, CancellationTokenSource> _running = new();

        public IReadOnlyCollection<string> ActiveServices => _running.Keys.ToList();

        public ServiceManager(ILogger<ServiceManager> logger, IConfiguration config, string? filePath = null)
        {
            _logger = logger;
            _config = config;
            _filePath = filePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "services.json");
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
            _running[info.DisplayName] = cts;
            _ = Task.Run(() => RunServiceLoop(info, cts.Token));
        }

        private void StopService(string name)
        {
            if (_running.TryGetValue(name, out var cts))
            {
                _logger.LogInformation("Stopping service: {name}", name);
                cts.Cancel();
                _running.Remove(name);
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
            foreach (var cts in _running.Values)
                cts.Cancel();
            _running.Clear();
        }
    }
}
