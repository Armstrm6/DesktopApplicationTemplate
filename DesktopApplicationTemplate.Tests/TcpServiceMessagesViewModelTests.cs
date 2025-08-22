using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.ViewModels;
using FluentAssertions;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class TcpServiceMessagesViewModelTests
{
    [Fact]
    public void DisplayLogs_RespectsLogLevelFilter()
    {
        var vm = new TcpServiceMessagesViewModel();
        vm.Logs.Add(new LogEntry { Message = "a", Level = DesktopApplicationTemplate.Core.Services.LogLevel.Debug });
        vm.Logs.Add(new LogEntry { Message = "b", Level = DesktopApplicationTemplate.Core.Services.LogLevel.Error });

        vm.LogLevelFilter = DesktopApplicationTemplate.Core.Services.LogLevel.Error;

        vm.DisplayLogs.Should().ContainSingle().Which.Message.Should().Be("b");
    }

    [Fact]
    public void ClearLogCommand_RemovesLogs()
    {
        var vm = new TcpServiceMessagesViewModel();
        vm.Logs.Add(new LogEntry { Message = "test" });

        vm.ClearLogCommand.Execute(null);

        vm.Logs.Should().BeEmpty();
    }
}
