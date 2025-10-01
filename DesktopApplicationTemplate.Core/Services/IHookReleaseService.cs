namespace DesktopApplicationTemplate.Core.Services;

/// <summary>
/// Provides a hook for releasing resources such as input hooks when the host shuts down.
/// </summary>
public interface IHookReleaseService
{
    /// <summary>
    /// Releases any active hooks or simulated input state.
    /// </summary>
    void Release();
}
