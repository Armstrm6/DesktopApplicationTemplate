using System.Runtime.Versioning;

namespace DesktopApplicationTemplate.Core.Services
{
    /// <summary>
    /// Specifies logging severity levels.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public enum LogLevel
    {
        Debug = 0,
        Warning = 1,
        Error = 2,
        Critical = 3
    }
}
