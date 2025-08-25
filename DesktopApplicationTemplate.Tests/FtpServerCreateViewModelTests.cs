using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.Core.Services;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class FtpServerCreateViewModelTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void SaveCommand_RaisesServerCreated()
    {
        var logger = new Mock<ILoggingService>();
        var vm = new FtpServerCreateViewModel(logger.Object)
        {
            ServiceName = "ftp",
            Port = 21,
            RootPath = "/tmp"
        };
        string? name = null;
        FtpServerOptions? options = null;
        vm.ServerCreated += (n, o) => { name = n; options = o; };

        vm.SaveCommand.Execute(null);

        Assert.Equal("ftp", name);
        Assert.NotNull(options);
        Assert.Equal(21, options!.Port);
        Assert.Equal("/tmp", options.RootPath);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void SettingInvalidPort_AddsError()
    {
        var vm = new FtpServerCreateViewModel();
        vm.Port = 0;
        Assert.True(vm.HasErrors);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void SettingEmptyServiceName_AddsError()
    {
        var vm = new FtpServerCreateViewModel();
        vm.ServiceName = string.Empty;
        Assert.True(vm.HasErrors);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void SaveCommand_DoesNotRaise_WhenInvalid()
    {
        var vm = new FtpServerCreateViewModel
        {
            ServiceName = string.Empty,
            Port = 21,
            RootPath = "/tmp"
        };
        var raised = false;
        vm.ServerCreated += (_, _) => raised = true;

        vm.SaveCommand.Execute(null);

        Assert.False(raised);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void AdvancedCommand_RaisesRequested()
    {
        var vm = new FtpServerCreateViewModel();
        var raised = false;
        vm.AdvancedConfigRequested += _ => raised = true;

        vm.AdvancedConfigCommand.Execute(null);

        Assert.True(raised);
        ConsoleTestLogger.LogPass();
    }
}
