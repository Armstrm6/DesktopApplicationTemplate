using DesktopApplicationTemplate.Models;
using DesktopApplicationTemplate.UI.Models;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.ViewModels.Tcp;
using DesktopApplicationTemplate.UI.Helpers;
using FluentAssertions;
using Xunit;
using System.Threading.Tasks;
using System.Linq;

namespace DesktopApplicationTemplate.Tests;

public class TcpServiceMessagesViewModelTests
{
    [Fact]
    public void DisplayLogs_RespectsLogLevelFilter()
    {
        var vm = new TcpServiceMessagesViewModel(new ServiceMessageTableViewModel(), new MessageRoutingService())
        {
            Logs =
            {
                new LogEntry { Message = "a", Level = DesktopApplicationTemplate.Core.Services.LogLevel.Debug },
                new LogEntry { Message = "b", Level = DesktopApplicationTemplate.Core.Services.LogLevel.Information },
                new LogEntry { Message = "c", Level = DesktopApplicationTemplate.Core.Services.LogLevel.Error }
            }
        };

        vm.LogLevelFilter = DesktopApplicationTemplate.Core.Services.LogLevel.Information;

        vm.DisplayLogs.Select(l => l.Message).Should().Equal(new[] { "b", "c" });
    }

    [Fact]
    public void ClearLogCommand_RemovesLogs()
    {
        var vm = new TcpServiceMessagesViewModel(new ServiceMessageTableViewModel(), new MessageRoutingService())
        {
            Logs = { new LogEntry { Message = "test" } }
        };

        vm.ClearLogCommand.Execute(null);

        vm.Logs.Should().BeEmpty();
    }

    [Fact]
    public void OpenAdvancedSettingsCommand_RaisesEvent()
    {
        var vm = new TcpServiceMessagesViewModel(new ServiceMessageTableViewModel(), new MessageRoutingService());
        var raised = false;
        vm.AdvancedSettingsRequested += (_, _) => raised = true;

        vm.OpenAdvancedSettingsCommand.Execute(null);

        raised.Should().BeTrue();
    }

    [Fact]
    public void UpdateNetworkSettings_SetsProperties()
    {
        var vm = new TcpServiceMessagesViewModel(new ServiceMessageTableViewModel(), new MessageRoutingService());

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
        var vm = new TcpServiceMessagesViewModel(new ServiceMessageTableViewModel(), new MessageRoutingService())
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
        vm.OutgoingResults.Should().ContainSingle(r => r.Contains("svc") && r.Contains("ok"));
    }

    [Fact]
    public void ServiceName_NoPriorMessage_UsesDefault()
    {
        var service = TestHelpers.CreateService(ServiceType.Tcp, "svc");
        service.SetPayload(new TcpServiceOptions());
        var routing = new MessageRoutingService();
        var vm = new TcpServiceMessagesViewModel(new ServiceMessageTableViewModel(), routing);
        vm.SetService(service);

        vm.TestMessage.Should().Be("svc-PEAK-123456789");
        routing.TryGetMessage(ServiceType.Tcp, "svc", out var message).Should().BeTrue();
        message.Should().Be("svc-PEAK-123456789");
    }

    [Fact]
    public void ServiceName_NoPriorMessage_UsesRoutingMessage()
    {
        var service = TestHelpers.CreateService(ServiceType.Tcp, "svc");
        service.SetPayload(new TcpServiceOptions());
        var routing = new MessageRoutingService();
        routing.UpdateMessage(ServiceType.Tcp, "svc", "last");
        var vm = new TcpServiceMessagesViewModel(new ServiceMessageTableViewModel(), routing);

        vm.SetService(service);

        vm.TestMessage.Should().Be("last");
    }

    [Fact]
    public void ServiceName_WithPriorMessage_UsesStored()
    {
        var service = TestHelpers.CreateService(ServiceType.Tcp, "svc");
        service.SetPayload(new TcpServiceOptions { LastTestMessage = "hello" });
        var vm = new TcpServiceMessagesViewModel(new ServiceMessageTableViewModel(), new MessageRoutingService());
        vm.SetService(service);

        vm.TestMessage.Should().Be("hello");
    }

    [Fact]
    public async Task SaveAsync_UpdatesLastTestMessage()
    {
        var options = new TcpServiceOptions();
        var service = TestHelpers.CreateService(ServiceType.Tcp, "svc");
        service.SetPayload(options);
        var vm = new TcpServiceMessagesViewModel(new ServiceMessageTableViewModel(), new MessageRoutingService());
        vm.SetService(service);
        vm.TestMessage = "test";

        await vm.SaveAsync();

        options.LastTestMessage.Should().Be("test");
    }

    [Fact]
    public async Task SaveAsync_UpdatesScript()
    {
        var options = new TcpServiceOptions();
        var service = TestHelpers.CreateService(ServiceType.Tcp, "svc");
        service.SetPayload(options);
        var vm = new TcpServiceMessagesViewModel(new ServiceMessageTableViewModel(), new MessageRoutingService());
        vm.SetService(service);
        vm.Script = "return message;";

        await vm.SaveAsync();

        options.Script.Should().Be("return message;");
    }

    [Fact]
    public async Task SaveAsync_ComputesOutputMessage()
    {
        var options = new TcpServiceOptions();
        var service = TestHelpers.CreateService(ServiceType.Tcp, "svc");
        service.SetPayload(options);
        var vm = new TcpServiceMessagesViewModel(new ServiceMessageTableViewModel(), new MessageRoutingService());
        vm.SetService(service);
        vm.Script = "string Process(string message) => message + \"!\";";
        vm.TestMessage = "hi";

        await vm.SaveAsync();

        vm.OutputMessage.Should().Be("hi!");
        options.OutputMessage.Should().Be("hi!");
    }

    [Fact]
    public void SetService_LoadsScript()
    {
        var service = TestHelpers.CreateService(ServiceType.Tcp, "svc");
        service.SetPayload(new TcpServiceOptions { Script = "return message;" });
        var vm = new TcpServiceMessagesViewModel(new ServiceMessageTableViewModel(), new MessageRoutingService());

        vm.SetService(service);

        vm.Script.Should().Be("return message;");
    }

    [Fact]
    public void SetService_NoScript_UsesDefault()
    {
        var service = TestHelpers.CreateService(ServiceType.Tcp, "svc");
        service.SetPayload(new TcpServiceOptions());
        var vm = new TcpServiceMessagesViewModel(new ServiceMessageTableViewModel(), new MessageRoutingService());

        vm.SetService(service);

        vm.Script.Should().Be(ScriptEditorViewModel.DefaultScript);
    }

    [Fact]
    public async Task SaveAsync_DefaultScript_NoProtectionLevelErrors()
    {
        var options = new TcpServiceOptions();
        var service = TestHelpers.CreateService(ServiceType.Tcp, "svc");
        service.SetPayload(options);
        var vm = new TcpServiceMessagesViewModel(new ServiceMessageTableViewModel(), new MessageRoutingService());
        vm.SetService(service);
        vm.TestMessage = "ping";

        await vm.SaveAsync();

        vm.OutputMessage.Should().Be("ping");
        vm.OutputMessage.Should().NotContain("protection level");
        options.OutputMessage.Should().Be("ping");
    }

    [Fact]
    public async Task SaveAsync_CustomScript_ReturnsTransformedMessage()
    {
        var options = new TcpServiceOptions();
        var service = TestHelpers.CreateService(ServiceType.Tcp, "svc");
        service.SetPayload(options);
        var vm = new TcpServiceMessagesViewModel(new ServiceMessageTableViewModel(), new MessageRoutingService());
        vm.SetService(service);
        vm.TestMessage = "ping";
        vm.Script = "string Process(string message)\n{\n    return message.ToUpperInvariant();\n}";

        await vm.SaveAsync();

        vm.OutputMessage.Should().Be("PING");
    }

    [Fact]
    public async Task ScriptEditor_RunCommand_ProcessesMessage()
    {
        var editor = new ScriptEditorViewModel();
        editor.TestMessage = "hello";

        await ((AsyncRelayCommand)editor.RunCommand).ExecuteAsync(null);

        editor.OutputMessage.Should().Be("hello");
    }

    [Fact]
    public async Task ScriptEditor_SaveCommand_RaisesRequestCloseWithLastMessage()
    {
        var editor = new ScriptEditorViewModel();
        editor.TestMessage = "msg";
        await ((AsyncRelayCommand)editor.RunCommand).ExecuteAsync(null);

        string? script = null;
        string? last = null;
        editor.RequestClose += (_, e) => { script = e.Script; last = e.TestMessage; };

        editor.SaveCommand.Execute(null);

        script.Should().Contain("Process");
        last.Should().Be("msg");
    }

    [Fact]
    public async Task OpenScriptEditor_RunCommand_UpdatesOutputMessage()
    {
        var vm = new TcpServiceMessagesViewModel(new ServiceMessageTableViewModel(), new MessageRoutingService());
        var editor = new ScriptEditorViewModel();

        void OnOutput(string output) => vm.OutputMessage = output; // internal setter
        editor.OutputGenerated += OnOutput;
        editor.TestMessage = "test";

        await ((AsyncRelayCommand)editor.RunCommand).ExecuteAsync(null);

        vm.OutputMessage.Should().Be("test");
        editor.OutputGenerated -= OnOutput;
    }

    [Fact]
    public async Task OpenScriptEditor_RunCommand_PersistsOutputAndTestMessage()
    {
        var options = new TcpServiceOptions();
        var service = TestHelpers.CreateService(ServiceType.Tcp, "svc");
        service.SetPayload(options);
        var routing = new MessageRoutingService();
        var vm = new TcpServiceMessagesViewModel(new ServiceMessageTableViewModel(), routing);
        vm.SetService(service);
        var editor = new ScriptEditorViewModel();

        void OnOutput(string output) => vm.OutputMessage = output; // internal setter
        editor.OutputGenerated += OnOutput;
        editor.TestMessage = "test";

        await ((AsyncRelayCommand)editor.RunCommand).ExecuteAsync(null);

        vm.OutputMessage.Should().Be("test");
        options.OutputMessage.Should().Be("test");
        options.LastTestMessage.Should().Be("test");
        routing.TryGetMessage(ServiceType.Tcp, "svc", out var message).Should().BeTrue();
        message.Should().Be("test");
        editor.OutputGenerated -= OnOutput;
    }

    [Fact]
    public void OutputMessage_SetSameValue_RaisesPropertyChanged()
    {
        var vm = new TcpServiceMessagesViewModel(new ServiceMessageTableViewModel(), new MessageRoutingService());
        string? property = null;
        vm.PropertyChanged += (_, e) => property = e.PropertyName;

        vm.OutputMessage = string.Empty;

        property.Should().Be(nameof(TcpServiceMessagesViewModel.OutputMessage));
    }
}
