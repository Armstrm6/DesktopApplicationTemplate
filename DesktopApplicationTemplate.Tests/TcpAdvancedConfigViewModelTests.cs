using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class TcpAdvancedConfigViewModelTests
{
    [Fact]
    public void SaveCommand_Raises_Saved_WithUpdatedOptions()
    {
        var options = new TcpServiceOptions { UseUdp = false, Mode = TcpServiceMode.Listening };
        var vm = new TcpAdvancedConfigViewModel();
        vm.Load(options);
        vm.UseUdp = true;
        vm.Mode = TcpServiceMode.Sending;
        vm.InputMessage = "hi";
        vm.Script = "string Process(string m){ return m + \"!\"; }";
        TcpServiceOptions? received = null;
        vm.Saved += o => received = o;

        vm.SaveCommand.Execute(null);

        Assert.NotNull(received);
        Assert.True(received!.UseUdp);
        Assert.Equal(TcpServiceMode.Sending, received.Mode);
        Assert.Equal("hi", received.InputMessage);
        Assert.Equal("string Process(string m){ return m + \"!\"; }", received.Script);
        Assert.Equal("hi!", received.OutputMessage);
    }

    [Fact]
    public void BackCommand_Raises_BackRequested()
    {
        var vm = new TcpAdvancedConfigViewModel();
        vm.Load(new TcpServiceOptions());
        var called = false;
        vm.BackRequested += () => called = true;

        vm.BackCommand.Execute(null);

        Assert.True(called);
    }

    [Fact]
    public void ScriptExecution_ComputesOutput()
    {
        var vm = new TcpAdvancedConfigViewModel();
        vm.Load(new TcpServiceOptions());
        vm.InputMessage = "abc";
        vm.Script = "string Process(string m){ return m.ToUpper(); }";

        Assert.Equal("ABC", vm.OutputMessage);
    }
}
