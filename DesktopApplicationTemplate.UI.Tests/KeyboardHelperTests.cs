using System.Windows.Input;
using DesktopApplicationTemplate.UI.Helpers;
using FluentAssertions;

namespace DesktopApplicationTemplate.Tests;

[Collection("WpfTests")]
public class KeyboardHelperTests
{
    [WindowsFact]
    public void ReleaseKeys_DoesNotThrow()
    {

        FluentActions.Invoking(() => KeyboardHelper.ReleaseKeys(Key.A))
            .Should().NotThrow();
    }
}
