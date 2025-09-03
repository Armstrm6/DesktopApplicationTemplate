using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Helpers;
using FluentAssertions;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class TcpServiceViewModelTests
{
    [Fact]
    public void SettingScriptContent_UpdatesMessagesViewModel()
    {
        var logger = new Mock<ILoggingService>();
        var helper = new SaveConfirmationHelper(logger.Object) { SaveConfirmationSuppressed = true };
        var messages = new TcpServiceMessagesViewModel();
        var vm = new TcpServiceViewModel(helper, messages);

        vm.ScriptContent = "test";

        messages.ScriptContent.Should().Be("test");
    }

    [Fact]
    public void SettingNetworkProperties_UpdatesMessagesViewModel()
    {
        var logger = new Mock<ILoggingService>();
        var helper = new SaveConfirmationHelper(logger.Object);
        var messages = new TcpServiceMessagesViewModel();
        var vm = new TcpServiceViewModel(helper, messages);

        vm.ComputerIp = "1.2.3.4";
        vm.ListeningPort = "1000";
        vm.ServerIp = "5.6.7.8";
        vm.ServerGateway = "9.9.9.9";
        vm.ServerPort = "2000";
        vm.IsUdp = true;

        messages.ComputerIp.Should().Be("1.2.3.4");
        messages.ListeningPort.Should().Be("1000");
        messages.ServerIp.Should().Be("5.6.7.8");
        messages.ServerGateway.Should().Be("9.9.9.9");
        messages.ServerPort.Should().Be("2000");
        messages.IsUdp.Should().BeTrue();
    }

    [Fact]
    public void SaveCommand_RaisesSaved()
    {
        var logger = new Mock<ILoggingService>();
        var helper = new SaveConfirmationHelper(logger.Object);
        var messages = new TcpServiceMessagesViewModel();
        var vm = new TcpServiceViewModel(helper, messages);
        var raised = false;
        vm.Saved += (_, _) => raised = true;

        vm.SaveCommand.Execute(null);

        raised.Should().BeTrue();
    }

    [Fact]
    public void BackCommand_RaisesBackRequested()
    {
        var logger = new Mock<ILoggingService>();
        var helper = new SaveConfirmationHelper(logger.Object);
        var messages = new TcpServiceMessagesViewModel();
        var vm = new TcpServiceViewModel(helper, messages);
        var raised = false;
        vm.BackRequested += (_, _) => raised = true;

        vm.BackCommand.Execute(null);

        raised.Should().BeTrue();
    }

    [Fact]
    public async Task TestScriptCommand_UpdatesOutputMessage()
    {
        var logger = new Mock<ILoggingService>();
        var helper = new SaveConfirmationHelper(logger.Object);
        var messages = new TcpServiceMessagesViewModel();
        var vm = new TcpServiceViewModel(helper, messages);
        vm.ScriptContent = "string Process(string m){ return m + \"!\"; }";
        vm.InputMessage = "abc";

        await ((AsyncRelayCommand)vm.TestScriptCommand).ExecuteAsync();

        vm.OutputMessage.Should().Be("abc!");
    }
}
