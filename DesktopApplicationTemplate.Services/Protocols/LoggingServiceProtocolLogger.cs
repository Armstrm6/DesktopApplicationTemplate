using System;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Core.Services.Protocols;

namespace DesktopApplicationTemplate.Services.Protocols;

/// <summary>
/// Bridges the protocol logging abstraction to the application logging service.
/// </summary>
public sealed class LoggingServiceProtocolLogger : IProtocolLogger
{
    private readonly ILoggingService loggingService;

    public LoggingServiceProtocolLogger(ILoggingService loggingService)
    {
        this.loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
    }

    /// <inheritdoc />
    public void LogInformation(IProtocolService protocolService, string message)
    {
        Log(protocolService, message, LogLevel.Information);
    }

    /// <inheritdoc />
    public void LogWarning(IProtocolService protocolService, string message)
    {
        Log(protocolService, message, LogLevel.Warning);
    }

    /// <inheritdoc />
    public void LogError(IProtocolService protocolService, Exception exception, string message)
    {
        var formattedMessage = FormatMessage(protocolService, message);
        loggingService.Log($"{formattedMessage} - {exception}", LogLevel.Error);
    }

    private void Log(IProtocolService protocolService, string message, LogLevel level)
    {
        loggingService.Log(FormatMessage(protocolService, message), level);
    }

    private static string FormatMessage(IProtocolService protocolService, string message)
    {
        var sourceName = protocolService?.Name ?? "UnknownProtocol";
        var safeMessage = string.IsNullOrWhiteSpace(message) ? "No message provided." : message;
        return $"[{sourceName}] {safeMessage}";
    }
}
