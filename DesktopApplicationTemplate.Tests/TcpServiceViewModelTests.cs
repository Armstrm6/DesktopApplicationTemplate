using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class TcpServiceViewModelTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void SettingScriptContent_UpdatesMessagesViewModel()
    {
        var logger = new Mock<ILoggingService>();
        var helper = new SaveConfirmationHelper(logger.Object);
        var messages = new TcpServiceMessagesViewModel();
        var vm = new TcpServiceViewModel(helper, messages);

        vm.ScriptContent = "test";

        messages.ScriptContent.Should().Be("test");
    }

    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
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
}
