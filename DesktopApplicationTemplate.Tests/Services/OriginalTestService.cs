using System;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.Tests.Services;

/// <summary>
/// Baseline service used to compare behaviors with <see cref="ComposedTestService"/>.
/// </summary>
public class OriginalTestService
{
    private readonly ILoggingService _logger;

    /// <summary>
    /// Gets the last validation error, if any.
    /// </summary>
    public string? LastError { get; private set; }

    /// <summary>
    /// Raised when the service successfully saves options.
    /// </summary>
    public event Action<string, TestServiceOptions>? ServiceSaved;

    public OriginalTestService(ILoggingService logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates the name and options, logging around save when valid.
    /// </summary>
    public bool Save(string? name, TestServiceOptions options)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            LastError = "Name is required";
            return false;
        }

        if (options.Port < 1 || options.Port > 65535)
        {
            LastError = "Port must be between 1 and 65535";
            return false;
        }

        _logger.Log($"Saving service {name}", LogLevel.Debug);
        ServiceSaved?.Invoke(name!, options);
        _logger.Log($"Saved service {name}", LogLevel.Debug);
        LastError = null;
        return true;
    }
}

