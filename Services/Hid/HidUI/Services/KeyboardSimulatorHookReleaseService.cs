using DesktopApplicationTemplate.Core.Services;

namespace DesktopApplicationTemplate.Services.Hid.UI.Services;

/// <summary>
/// Releases simulated keyboard hooks by delegating to <see cref="KeyboardSimulator"/>.
/// </summary>
public sealed class KeyboardSimulatorHookReleaseService : IHookReleaseService
{
    public void Release()
    {
        KeyboardSimulator.Reset();
    }
}
