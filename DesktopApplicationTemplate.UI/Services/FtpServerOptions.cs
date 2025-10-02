namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Configuration options for hosting an FTP server.
    /// </summary>
    public class FtpServerOptions
    {
        /// <summary>
        /// Port number on which the server listens.
        /// </summary>
        public int Port { get; set; } = 21;

        /// <summary>
        /// Root directory for FTP file storage.
        /// </summary>
        public string RootPath { get; set; } = string.Empty;

        /// <summary>
        /// Allow anonymous connections.
        /// </summary>
        public bool AllowAnonymous { get; set; }

        /// <summary>
        /// Username for authenticated access.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Password for authenticated access.
        /// </summary>
        public string? Password { get; set; }
    }
}
