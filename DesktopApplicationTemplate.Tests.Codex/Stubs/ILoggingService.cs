namespace DesktopApplicationTemplate.UI.Services
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
