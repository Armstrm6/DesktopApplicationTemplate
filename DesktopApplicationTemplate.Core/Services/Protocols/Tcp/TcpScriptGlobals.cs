namespace DesktopApplicationTemplate.Core.Services.Protocols.Tcp;

/// <summary>
/// Provides the global variables exposed to TCP C# scripts.
/// </summary>
public sealed class TcpScriptGlobals
{
    /// <summary>
    /// Gets or sets the inbound message provided to the script.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
