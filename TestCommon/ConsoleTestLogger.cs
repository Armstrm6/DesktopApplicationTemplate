using System;
using System.Runtime.CompilerServices;

namespace DesktopApplicationTemplate.Tests;

public static class ConsoleTestLogger
{
    public static void LogPass([CallerMemberName] string? testName = null)
    {
        var prevColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\u001b[1m[Tests] " + testName + " PASSED\u001b[0m");
        Console.ForegroundColor = prevColor;
    }
}
