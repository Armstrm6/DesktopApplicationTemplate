using DesktopApplicationTemplate.UI.Services;
using System.Collections.Generic;

namespace DesktopApplicationTemplate.Tests
{
    public class TestLogger : ILoggingService
    {
        public List<(string Message, LogLevel Level)> Entries { get; } = new();

        public void Log(string message, LogLevel level)
        {
            Entries.Add((message, level));
        }
    }
}
