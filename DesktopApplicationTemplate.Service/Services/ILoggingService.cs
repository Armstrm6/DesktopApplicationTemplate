using System;

namespace DesktopApplicationTemplate.UI.Services
{
    public interface ILoggingService
    {
        /// <summary>
        /// Gets or sets the minimum log level that will be processed.
        /// </summary>
        LogLevel MinimumLevel { get; set; }

        /// <summary>
        /// Logs a message at the specified level.
        /// </summary>
        void Log(string message, LogLevel level);
    }

    public enum LogLevel
    {
        Debug,
        Warning,
        Error,
        Critical
    }
}
