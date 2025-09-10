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
        Information = 1,
        Warning = 2,
        Error = 3,
        Critical = 4
    }
}
