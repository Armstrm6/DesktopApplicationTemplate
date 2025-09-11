namespace DesktopApplicationTemplate.Models
{
    /// <summary>
    /// Identifies the supported service categories within the application.
    /// </summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(DesktopApplicationTemplate.Core.Converters.ServiceTypeJsonConverter))]
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
