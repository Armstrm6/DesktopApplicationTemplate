using System;
using System.Threading;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services.Protocols;

namespace DesktopApplicationTemplate.Core.Services.Protocols.Heartbeat;

/// <summary>
/// Implements <see cref="IHeartbeatService"/> using <see cref="PeriodicTimer"/>.
/// </summary>
public sealed class HeartbeatService : IHeartbeatService
{
    private readonly IProtocolLogger? _logger;
    private HeartbeatServiceOptions? _options;
    private TimeSpan _interval = TimeSpan.FromSeconds(5);
    private CancellationTokenSource? _timerCts;
    private Task? _timerTask;
    private string _name = nameof(HeartbeatService);
    private bool _isRunning;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeartbeatService"/> class.
    /// </summary>
    public HeartbeatService(IProtocolLogger? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public event EventHandler<string>? HeartbeatGenerated;

    /// <inheritdoc />
    public string Name => _name;

    /// <inheritdoc />
    public bool IsRunning => _isRunning;

    /// <inheritdoc />
    public async Task ConfigureAsync(string serviceName, HeartbeatServiceOptions options, TimeSpan interval, CancellationToken cancellationToken = default)
    {
        _name = serviceName ?? nameof(HeartbeatService);
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _interval = interval <= TimeSpan.Zero ? TimeSpan.FromSeconds(5) : interval;

        _logger?.LogInformation(this, $"Heartbeat configured with interval {_interval}");
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public string BuildHeartbeatMessage(HeartbeatServiceOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var message = options.BaseMessage ?? string.Empty;
        if (options.IncludePing)
        {
            message += " | PING";
        }

        if (options.IncludeStatus)
        {
            message += " | STATUS";
        }

        _logger?.LogInformation(this, $"Heartbeat built: {message}");
        return message;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_options is null)
        {
            throw new InvalidOperationException("ConfigureAsync must be called before StartAsync.");
        }

        if (_isRunning)
        {
            return Task.CompletedTask;
        }

        _isRunning = true;
        _timerCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var localToken = _timerCts.Token;
        var options = _options;
        var interval = _interval;

        _logger?.LogInformation(this, "Heartbeat timer starting");

        _timerTask = Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(interval);
            while (await timer.WaitForNextTickAsync(localToken).ConfigureAwait(false))
            {
                var payload = BuildHeartbeatMessage(options);
                _logger?.LogInformation(this, "Heartbeat emitted");
                HeartbeatGenerated?.Invoke(this, payload);
            }
        }, localToken);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
        {
            return;
        }

        _logger?.LogInformation(this, "Heartbeat timer stopping");
        _isRunning = false;
        _timerCts?.Cancel();

        if (_timerTask is not null)
        {
            try
            {
                await _timerTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // expected when stopping
            }
        }

        _timerTask = null;
        _timerCts?.Dispose();
        _timerCts = null;
    }
}
