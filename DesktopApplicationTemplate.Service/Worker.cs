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
        private readonly IConfiguration _config;

        public string HeartbeatMessage { get; private set; }
        public int IntervalSeconds { get; private set; }

        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            var hbSection = _config.GetSection("Heartbeat");
            HeartbeatMessage = hbSection.GetValue<string>("Message", "PING");
            IntervalSeconds = hbSection.GetValue<int>("IntervalSeconds", 30);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service starting at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Heartbeat: {message}", HeartbeatMessage);
                await Task.Delay(TimeSpan.FromSeconds(IntervalSeconds), stoppingToken);
            }

            _logger.LogInformation("Service stopping at: {time}", DateTimeOffset.Now);
        }
    }
}
