namespace DesktopApplicationTemplate.Models
{
    public class UserSettings
    {
        public bool DarkTheme { get; set; }
        public bool AutoCheckUpdates { get; set; }
        public bool RunUIOnStartup { get; set; }
        public bool RunServicesOnStartup { get; set; }
        public bool LogTcpMessages { get; set; } = true;
        public bool FirstRun { get; set; } = true;
        public bool SuppressSaveConfirmation { get; set; }
        public bool SuppressCloseConfirmation { get; set; }
    }
}
