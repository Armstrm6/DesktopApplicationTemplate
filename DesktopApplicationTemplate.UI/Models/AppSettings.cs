namespace DesktopApplicationTemplate.UI.Models
{
    /// <summary>
    /// Represents application-level settings loaded from configuration.
    /// </summary>
    public class AppSettings
    {
        /// <summary>Gets or sets the active environment name.</summary>
        public string Environment { get; set; } = "Development";

        /// <summary>Gets or sets the default server IP.</summary>
        public string ServerIP { get; set; } = "127.0.0.1";

        /// <summary>Gets or sets the default server port.</summary>
        public int ServerPort { get; set; } = 9000;

        /// <summary>Gets or sets the log level as text.</summary>
        public string LogLevel { get; set; } = "Debug";

        /// <summary>Gets or sets a value indicating whether services should start automatically.</summary>
        public bool AutoStart { get; set; } = false;

        /// <summary>Gets or sets the path for the default C# script.</summary>
        public string DefaultCSharpScriptPath { get; set; } = string.Empty;
    }
}

