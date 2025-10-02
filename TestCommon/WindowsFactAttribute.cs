using System;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class WindowsFactAttribute : FactAttribute
{
    public WindowsFactAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            Skip = "Windows-only test";
        }
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class WindowsTheoryAttribute : TheoryAttribute
{
    public WindowsTheoryAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            Skip = "Windows-only test";
        }
    }
}
