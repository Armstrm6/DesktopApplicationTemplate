using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _config;
        
        public string HeartbeatMessage { get; private set; } = string.Empty;
        public int IntervalSeconds { get; private set; }

        public Worker(ILogger<Worker> logger, ILoggerFactory loggerFactory, IConfiguration config)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _config = config;
            var hbSection = _config.GetSection("Heartbeat");
            HeartbeatMessage = hbSection.GetValue<string>("Message") ?? "PING";
            IntervalSeconds = hbSection.GetValue<int>("IntervalSeconds", 30);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service starting at: {time}", DateTimeOffset.Now);
            _logger.LogDebug("Heartbeat interval set to {interval}s with message '{message}'", IntervalSeconds, HeartbeatMessage);

            using var manager = new ServiceManager(_loggerFactory.CreateLogger<ServiceManager>(), _config);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    manager.Sync();
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
                // service is stopping
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unhandled exception in Worker");
            }
            finally
            {
                _logger.LogInformation("Service stopping at: {time}", DateTimeOffset.Now);
            }
        }
    }
}
