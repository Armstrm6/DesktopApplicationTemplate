using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.Core.Services;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class FtpServerEditViewModelTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void Constructor_LoadsExistingSettings()
    {
        var options = new FtpServerOptions { Port = 22, RootPath = "/srv" };
        var vm = new FtpServerEditViewModel("ftp", options);
        Assert.Equal("ftp", vm.ServiceName);
        Assert.Equal(22, vm.Port);
        Assert.Equal("/srv", vm.RootPath);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void SaveCommand_RaisesServerUpdated()
    {
        var logger = new Mock<ILoggingService>();
        var options = new FtpServerOptions { Port = 21, RootPath = "/tmp" };
        var vm = new FtpServerEditViewModel("ftp", options, logger.Object)
        {
            Port = 2121,
            RootPath = "/var",
            ServiceName = "ftp2"
        };
        string? name = null;
        FtpServerOptions? updated = null;
        vm.ServerUpdated += (n, o) => { name = n; updated = o; };

        vm.SaveCommand.Execute(null);

        Assert.Equal("ftp2", name);
        Assert.NotNull(updated);
        Assert.Equal(2121, options.Port);
        Assert.Equal("/var", options.RootPath);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    [TestCategory("CodexSafe")]
    [TestCategory("WindowsSafe")]
    public void CancelCommand_RaisesCancelled()
    {
        var options = new FtpServerOptions();
        var vm = new FtpServerEditViewModel("ftp", options);
        var cancelled = false;
        vm.Cancelled += () => cancelled = true;

        vm.CancelCommand.Execute(null);

        Assert.True(cancelled);
        ConsoleTestLogger.LogPass();
    }
}

