using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ServiceManager _serviceManager;

        public Worker(ILogger<Worker> logger, ServiceManager serviceManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service starting at: {time}", DateTimeOffset.Now);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await _serviceManager.SyncAsync(stoppingToken).ConfigureAwait(false);
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
                await _serviceManager.StopAllAsync(stoppingToken).ConfigureAwait(false);
                _logger.LogInformation("Service stopping at: {time}", DateTimeOffset.Now);
            }
        }
    }
}
