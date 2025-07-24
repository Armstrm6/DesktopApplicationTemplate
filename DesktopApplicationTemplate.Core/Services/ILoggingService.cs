using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApplicationTemplate.Core.Services
{
    public interface ILoggingService
    {
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
