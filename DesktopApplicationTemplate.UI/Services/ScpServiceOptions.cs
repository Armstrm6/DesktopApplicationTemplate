namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Configuration options for SCP services.
    /// </summary>
    public class ScpServiceOptions
    {
        /// <summary>
        /// SCP host name or IP address.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// SCP port number.
        /// </summary>
        public int Port { get; set; } = 22;

        /// <summary>
        /// Username for authentication.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Password for authentication.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Local file path to upload.
        /// </summary>
        public string LocalPath { get; set; } = string.Empty;

        /// <summary>
        /// Remote destination path.
        /// </summary>
        public string RemotePath { get; set; } = string.Empty;
    }
}
