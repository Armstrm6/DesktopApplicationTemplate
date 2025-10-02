using System;
using Microsoft.Extensions.Logging;

namespace DesktopApplicationTemplate.Services;

/// <summary>
/// Provides a base class with a strongly typed logger.
/// </summary>
/// <typeparam name="T">Implementing type.</typeparam>
public abstract class LoggedServiceBase<T>
{
    protected LoggedServiceBase(ILogger<T> logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Gets the logger instance.</summary>
    protected ILogger<T> Logger { get; }
}
