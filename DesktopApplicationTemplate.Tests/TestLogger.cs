using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using System.Collections.Generic;

namespace DesktopApplicationTemplate.Tests
{
    public class TestLogger : ILoggingService
    {
        public List<(string Message, LogLevel Level)> Entries { get; } = new();
        public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;
        public event Action<LogEntry>? LogAdded;
        public void Log(string message, LogLevel level)
        {
            Entries.Add((message, level));
            LogAdded?.Invoke(new LogEntry { Message = message, Level = level });
        }

        public void Reload()
        {
        }
    }
}
