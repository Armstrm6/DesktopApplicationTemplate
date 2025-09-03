using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Service.Services;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class FtpServerEditViewModelTests
{
    [Fact]
    public void Constructor_LoadsExistingSettings()
    {
        var options = new FtpServerOptions { Port = 22, RootPath = "/srv" };
        IServiceRule rule = new ServiceRule();
        var vm = new FtpServerEditViewModel(rule, "ftp", options);
        Assert.Equal("ftp", vm.ServiceName);
        Assert.Equal(22, vm.Port);
        Assert.Equal("/srv", vm.RootPath);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    public void SaveCommand_RaisesServerSaved()
    {
        var logger = new Mock<ILoggingService>();
        var options = new FtpServerOptions { Port = 21, RootPath = "/tmp" };
        IServiceRule rule = new ServiceRule();
        var vm = new FtpServerEditViewModel(rule, "ftp", options, logger.Object)
        {
            Port = 2121,
            RootPath = "/var",
            ServiceName = "ftp2"
        };
        string? name = null;
        FtpServerOptions? updated = null;
        vm.ServiceSaved += (n, o) => { name = n; updated = o; };

        vm.SaveCommand.Execute(null);

        Assert.Equal("ftp2", name);
        Assert.NotNull(updated);
        Assert.Equal(2121, options.Port);
        Assert.Equal("/var", options.RootPath);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    public void CancelCommand_RaisesEditCancelled()
    {
        var options = new FtpServerOptions();
        IServiceRule rule = new ServiceRule();
        var vm = new FtpServerEditViewModel(rule, "ftp", options);
        var cancelled = false;
        vm.EditCancelled += () => cancelled = true;

        vm.CancelCommand.Execute(null);

        Assert.True(cancelled);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    public void SettingInvalidPort_AddsError()
    {
        var options = new FtpServerOptions();
        IServiceRule rule = new ServiceRule();
        var vm = new FtpServerEditViewModel(rule, "ftp", options);
        vm.Port = 0;
        Assert.True(vm.HasErrors);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    public void SaveCommand_DoesNotRaise_WhenInvalid()
    {
        var options = new FtpServerOptions();
        IServiceRule rule = new ServiceRule();
        var vm = new FtpServerEditViewModel(rule, "ftp", options)
        {
            Port = 0,
            RootPath = "/tmp"
        };
        var raised = false;
        vm.ServiceSaved += (_, _) => raised = true;

        vm.SaveCommand.Execute(null);

        Assert.False(raised);
        ConsoleTestLogger.LogPass();
    }
}

