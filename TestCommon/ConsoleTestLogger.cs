using System;
using System.Runtime.CompilerServices;

namespace DesktopApplicationTemplate.Tests;

public static class ConsoleTestLogger
{
    public static void LogPass([CallerMemberName] string? testName = null)
    {
        var prevColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[Tests] {testName} PASSED");
        Console.ForegroundColor = prevColor;
    }
}
