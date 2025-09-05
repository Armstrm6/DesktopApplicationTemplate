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
        var vm = new TcpServiceMessagesViewModel
        {
            Logs =
            {
                new LogEntry { Message = "a", Level = DesktopApplicationTemplate.Core.Services.LogLevel.Debug },
                new LogEntry { Message = "b", Level = DesktopApplicationTemplate.Core.Services.LogLevel.Error }
            }
        };

        vm.LogLevelFilter = DesktopApplicationTemplate.Core.Services.LogLevel.Error;

        vm.DisplayLogs.Should().ContainSingle().Which.Message.Should().Be("b");
    }

    [Fact]
    public void ClearLogCommand_RemovesLogs()
    {
        var vm = new TcpServiceMessagesViewModel
        {
            Logs = { new LogEntry { Message = "test" } }
        };

        vm.ClearLogCommand.Execute(null);

        vm.Logs.Should().BeEmpty();
    }

    [Fact]
    public void OpenAdvancedSettingsCommand_RaisesEvent()
    {
        var vm = new TcpServiceMessagesViewModel();
        var raised = false;
        vm.AdvancedSettingsRequested += (_, _) => raised = true;

        vm.OpenAdvancedSettingsCommand.Execute(null);

        raised.Should().BeTrue();
    }

    [Fact]
    public void UpdateScript_SetsScriptContent()
    {
        var vm = new TcpServiceMessagesViewModel();

        vm.UpdateScript("print('hi')");

        vm.ScriptContent.Should().Be("print('hi')");
    }

    [Fact]
    public void UpdateNetworkSettings_SetsProperties()
    {
        var vm = new TcpServiceMessagesViewModel();

        vm.UpdateNetworkSettings("1.1.1.1", "1000", "2.2.2.2", "3.3.3.3", "2000", true);

        vm.ComputerIp.Should().Be("1.1.1.1");
        vm.ListeningPort.Should().Be("1000");
        vm.ServerIp.Should().Be("2.2.2.2");
        vm.ServerGateway.Should().Be("3.3.3.3");
        vm.ServerPort.Should().Be("2000");
        vm.IsUdp.Should().BeTrue();
    }

    [Fact]
    public void MessageCollections_ExposeGroupedData()
    {
        var vm = new TcpServiceMessagesViewModel
        {
            Messages =
            {
                new TcpMessageRow
                {
                    IncomingMessage = "in",
                    IncomingIp = "1.1.1.1",
                    OutgoingMessage = "out",
                    ConnectedService = "svc",
                    Result = "ok"
                }
            }
        };

        vm.IncomingData.Should().ContainSingle(d => d.Contains("in"));
        vm.ScriptModifications.Should().ContainSingle("out");
        vm.OutgoingResults.Should().ContainSingle(r => r.Contains("svc") && r.Contains("ok"));
    }
}
