using System;
using System.Threading.Tasks;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols;
using DesktopApplicationTemplate.Core.Services.Protocols.FileObserver;
using DesktopApplicationTemplate.Core.Services.Protocols.Ftp;
using DesktopApplicationTemplate.Core.Services.Protocols.Heartbeat;
using DesktopApplicationTemplate.Core.Services.Protocols.Http;
using DesktopApplicationTemplate.Core.Services.Protocols.Tcp;
using DesktopApplicationTemplate.Models;

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
        var safeMessage = string.IsNullOrWhiteSpace(message)
            ? "No message provided."
            : message.Trim();

        var sourceName = protocolService?.Name ?? "UnknownProtocol";

        if (TryParseEmbeddedContext(sourceName, out var embeddedType, out var embeddedName))
        {
            return $"{embeddedType}.{embeddedName}.{safeMessage}";
        }

        if (TryResolveServiceType(protocolService, out var resolvedType))
        {
            var normalizedName = SanitizeServiceName(sourceName);
            return $"{resolvedType}.{normalizedName}.{safeMessage}";
        }

        var normalizedSource = SanitizeServiceName(sourceName);
        return $"{normalizedSource}.{safeMessage}";
    }

    private static bool TryParseEmbeddedContext(string sourceName, out string serviceType, out string serviceName)
    {
        serviceType = string.Empty;
        serviceName = string.Empty;

        if (string.IsNullOrWhiteSpace(sourceName))
        {
            return false;
        }

        var firstDot = sourceName.IndexOf('.');
        if (firstDot <= 0)
        {
            return false;
        }

        var potentialType = sourceName[..firstDot];

        if (!Enum.TryParse(potentialType, ignoreCase: true, out ServiceType parsedType))
        {
            return false;
        }

        var remaining = sourceName[(firstDot + 1)..];
        if (string.IsNullOrWhiteSpace(remaining))
        {
            return false;
        }

        serviceType = parsedType.ToString();
        serviceName = remaining.Trim();
        return true;
    }

    private static bool TryResolveServiceType(IProtocolService? protocolService, out string serviceType)
    {
        serviceType = string.Empty;

        if (protocolService is null)
        {
            return false;
        }

        if (protocolService is ITcpRuntime)
        {
            serviceType = ServiceType.Tcp.ToString();
            return true;
        }

        if (protocolService is IHttpClientService)
        {
            serviceType = ServiceType.Http.ToString();
            return true;
        }

        if (protocolService is IFileObserverService)
        {
            serviceType = ServiceType.FileObserver.ToString();
            return true;
        }

        if (protocolService is IFtpClientService or IFtpServerService)
        {
            serviceType = ServiceType.Ftp.ToString();
            return true;
        }

        if (protocolService is IHeartbeatService)
        {
            serviceType = ServiceType.Heartbeat.ToString();
            return true;
        }

        return false;
    }

    private static string SanitizeServiceName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "UnnamedService";
        }

        var trimmed = value.Trim();
        return trimmed.Replace('\r', ' ').Replace('\n', ' ').Replace('.', '_');
    }
}
