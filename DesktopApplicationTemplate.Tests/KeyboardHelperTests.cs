using System.Windows.Input;
using DesktopApplicationTemplate.UI.Helpers;
using FluentAssertions;

namespace DesktopApplicationTemplate.Tests;

public class KeyboardHelperTests
{
    [SkippableFact]
    public void ReleaseKeys_DoesNotThrow()
    {
        Skip.IfNot(OperatingSystem.IsWindows(), "Requires Windows desktop runtime");

        FluentActions.Invoking(() => KeyboardHelper.ReleaseKeys(Key.A))
            .Should().NotThrow();
    }
}
