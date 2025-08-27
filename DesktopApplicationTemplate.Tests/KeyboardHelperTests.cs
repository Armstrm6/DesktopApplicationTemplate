using System.Windows.Input;
using DesktopApplicationTemplate.UI.Helpers;
using FluentAssertions;

namespace DesktopApplicationTemplate.Tests;

public class KeyboardHelperTests
{
    [Fact]
    public void ReleaseKeys_DoesNotThrow()
    {
        if (!OperatingSystem.IsWindows())
            return;

        FluentActions.Invoking(() => KeyboardHelper.ReleaseKeys(Key.A))
            .Should().NotThrow();
    }
}
