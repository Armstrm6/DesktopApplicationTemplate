using System.Reflection;
using System.Windows.Input;

namespace DesktopApplicationTemplate.Tests;

internal static class MouseButtonEventArgsFactory
{
    public static MouseButtonEventArgs Create(MouseButton button, int clickCount)
    {
        var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, button);
        typeof(MouseButtonEventArgs)
            .GetProperty("ClickCount", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(args, clickCount);
        return args;
    }
}
