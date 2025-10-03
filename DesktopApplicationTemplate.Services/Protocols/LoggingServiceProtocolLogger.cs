using System;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols;

namespace DesktopApplicationTemplate.Services.Protocols;

/// <summary>
/// Bridges the protocol logging abstraction to the application logging service.
/// </summary>
public sealed class LoggingServiceProtocolLogger : IProtocolLogger
{
    private readonly ILoggingService _loggingService;
    private readonly IProtocolEventPublisher? _eventPublisher;

    public LoggingServiceProtocolLogger(ILoggingService loggingService, IProtocolEventPublisher? eventPublisher = null)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        _eventPublisher = eventPublisher;
    }

    /// <inheritdoc />
    public void LogInformation(IProtocolService protocolService, string message)
    {
        Log(protocolService, message, LogLevel.Information, null);
    }

    /// <inheritdoc />
    public void LogWarning(IProtocolService protocolService, string message)
    {
        Log(protocolService, message, LogLevel.Warning, null);
    }

    /// <inheritdoc />
    public void LogError(IProtocolService protocolService, Exception exception, string message)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        Log(protocolService, message, LogLevel.Error, exception);
    }

    private void Log(IProtocolService? protocolService, string message, LogLevel level, Exception? exception)
    {
        var formattedMessage = FormatMessage(protocolService, message);
        if (exception is null)
        {
            _loggingService.Log(formattedMessage, level);
        }
        else
        {
            _loggingService.Log($"{formattedMessage} - {exception}", level);
        }

        PublishEvent(protocolService, level, formattedMessage, exception);
    }

    private void PublishEvent(IProtocolService? protocolService, LogLevel level, string message, Exception? exception)
    {
        if (_eventPublisher is null)
        {
            return;
        }

        var protocolName = protocolService?.Name ?? "UnknownProtocol";
        var evt = new ProtocolLogEvent(protocolName, level, message, exception);
        var task = _eventPublisher.PublishAsync(evt);

        if (task.IsCompletedSuccessfully)
        {
            return;
        }

        task.ContinueWith(
            completed =>
            {
                if (completed.IsFaulted)
                {
                    _loggingService.Log(
                        $"Failed to publish protocol log event for {protocolName}: {completed.Exception?.GetBaseException().Message}",
                        LogLevel.Warning);
                }
            },
            TaskScheduler.Default);
    }

    private static string FormatMessage(IProtocolService? protocolService, string message)
    {
        var sourceName = protocolService?.Name ?? "UnknownProtocol";
        var safeMessage = string.IsNullOrWhiteSpace(message) ? "No message provided." : message;
        return $"[{sourceName}] {safeMessage}";
    }
}
