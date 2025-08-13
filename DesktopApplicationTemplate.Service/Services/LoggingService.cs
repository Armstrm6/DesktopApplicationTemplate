using System;
using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.Service.Services
{
    public class LoggingService : ILoggingService
    {
        public void Log(string message, LogLevel level)
        {
            Console.WriteLine($"[{level}] {message}");
        }
    }
}
