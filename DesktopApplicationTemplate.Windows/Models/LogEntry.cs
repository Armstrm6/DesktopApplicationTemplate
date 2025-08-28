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
        /// Display color for the entry.
        /// </summary>
        public System.Windows.Media.Brush Color { get; set; } = System.Windows.Media.Brushes.Black;
    }
}
