using System;
using DesktopApplicationTemplate.Models;
namespace DesktopApplicationTemplate.Core.Services
{
    /// <summary>
    /// Provides logging capabilities for services and view models.
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// Minimum level that will be processed.
        /// </summary>
        LogLevel MinimumLevel { get; set; }

        /// <summary>
        /// Raised when a new log entry is added.
        /// </summary>
        event Action<LogEntry> LogAdded;

        /// <summary>
        /// Records a message with the specified severity.
        /// </summary>
        void Log(string message, LogLevel level);

        /// <summary>
        /// Reloads existing log entries from persistence if supported.
        /// </summary>
        void Reload();
    }
}
