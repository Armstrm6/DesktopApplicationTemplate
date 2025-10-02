using System;

namespace DesktopApplicationTemplate.UI.Services
{
    /// <summary>
    /// Configuration options for an HTTP service.
    /// </summary>
    public class HttpServiceOptions
    {
        /// <summary>
        /// Base URL for HTTP requests.
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Optional username for basic authentication.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Optional password for basic authentication.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Optional path to a client TLS certificate.
        /// </summary>
        public string? ClientCertificatePath { get; set; }
    }
}
