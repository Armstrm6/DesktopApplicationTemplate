using System.IO;
using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Services;
using FluentAssertions;

namespace DesktopApplicationTemplate.Tests;

public class LoggingServiceTests
{
    [Fact]
    public void Log_InformationLevel_UsesBlueColor()
    {
        var temp = Path.GetTempFileName();
        var service = new LoggingService(new NullRichTextLogger(), temp);
        LogEntry? entry = null;
        service.LogAdded += e => entry = e;

        service.Log("test", LogLevel.Information);

        entry.Should().NotBeNull();
        entry!.Level.Should().Be(LogLevel.Information);
        entry.Color.Should().Be("#0000FF");
    }
}
