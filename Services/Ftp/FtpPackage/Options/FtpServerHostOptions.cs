using System.ComponentModel.DataAnnotations;

namespace DesktopApplicationTemplate.Services.Ftp.Options;

/// <summary>
/// Configuration for hosting the built-in FTP server.
/// </summary>
public class FtpServerHostOptions
{
    private const int DefaultPort = 21;

    /// <summary>
    /// Gets or sets the address the server listens on.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the port exposed by the FTP server.
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; set; } = DefaultPort;

    /// <summary>
    /// Gets or sets the root directory shared by the FTP server.
    /// </summary>
    public string RootPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether anonymous logins are permitted.
    /// </summary>
    public bool AllowAnonymous { get; set; }

    /// <summary>
    /// Gets or sets the username accepted for authenticated access.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password accepted for authenticated access.
    /// </summary>
    public string? Password { get; set; }
}
