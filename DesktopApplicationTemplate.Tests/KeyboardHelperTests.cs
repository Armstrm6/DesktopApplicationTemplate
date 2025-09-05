using System.Windows.Input;
using DesktopApplicationTemplate.UI.Helpers;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class KeyboardHelperTests
{
    [WindowsFact]
    public void PressAndRelease_RestoresState()
    {
        KeyboardHelper.PressKey(Key.A);
        Assert.True(KeyboardHelper.IsPressed(Key.A));

        KeyboardHelper.ReleaseKey(Key.A);
        Assert.False(KeyboardHelper.IsPressed(Key.A));
        ConsoleTestLogger.LogPass();
    }

    [WindowsFact]
    public void ReleaseAll_ClearsAllKeys()
    {
        KeyboardHelper.PressKey(Key.A, Key.B);
        Assert.True(KeyboardHelper.IsPressed(Key.A));
        Assert.True(KeyboardHelper.IsPressed(Key.B));

        KeyboardHelper.ReleaseAll();
        Assert.False(KeyboardHelper.IsPressed(Key.A));
        Assert.False(KeyboardHelper.IsPressed(Key.B));
        ConsoleTestLogger.LogPass();
    }
}
