using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.Models
{
    public class LogEntry
    {
        public LogLevel Level { get; set; } = LogLevel.Debug;
        public string Message { get; set; } = string.Empty;
        public System.Windows.Media.Brush Color { get; set; } = System.Windows.Media.Brushes.Black;
    }
}
