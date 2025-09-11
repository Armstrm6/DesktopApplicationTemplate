namespace DesktopApplicationTemplate.Models
{
    /// <summary>
    /// Identifies the supported service categories within the application.
    /// </summary>
    public enum ServiceType
    {
        Mqtt,
        Http,
        Ftp,
        Hid,
        Csv,
        FileObserver,
        Scp,
        Tcp,
        Heartbeat
    }
}
