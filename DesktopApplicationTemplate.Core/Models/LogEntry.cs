using System.Runtime.Versioning;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.Models
{
    /// <summary>
    /// Represents a single log entry.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class LogEntry
    {
        /// <summary>
        /// Severity level of the entry.
        /// </summary>
        public LogLevel Level { get; set; } = LogLevel.Debug;

        /// <summary>
        /// Formatted log message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Display color for the entry in a cross-platform format (e.g., "#FF0000").
        /// </summary>
        public string Color { get; set; } = "#000000";
    }
}
