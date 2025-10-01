using System;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.Services.Common.Runtime;

internal sealed class HeartbeatRuntimeService : IServiceRuntime
{
    private readonly ILogger _logger;
    private readonly HeartbeatRuntimeOptions _options;
    private readonly string _displayName;
    private CancellationTokenSource? _linkedSource;
    private Task? _loopTask;

    public HeartbeatRuntimeService(ILogger logger, HeartbeatRuntimeOptions options, string displayName)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _displayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting heartbeat runtime for {DisplayName}", _displayName);
        _linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loopTask = Task.Run(() => RunLoopAsync(_linkedSource.Token), CancellationToken.None);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_linkedSource is null)
        {
            return;
        }

        _logger.LogInformation("Stopping heartbeat runtime for {DisplayName}", _displayName);
        _linkedSource.Cancel();

        if (_loopTask is not null)
        {
            try
            {
                await Task.WhenAny(_loopTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation while waiting for shutdown.
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        _linkedSource?.Dispose();
        return ValueTask.CompletedTask;
    }

    private async Task RunLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("[{DisplayName}] {Message}", _displayName, _options.Message);

                try
                {
                    var delay = TimeSpan.FromSeconds(Math.Max(1, _options.IntervalSeconds));
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation requests.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Heartbeat runtime for {DisplayName} encountered an error.", _displayName);
        }
    }
}
