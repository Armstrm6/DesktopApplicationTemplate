using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.Service.Services;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class FtpServerCreateViewModelTests
{
    [Fact]
    public void SaveCommand_RaisesServerSaved()
    {
        var logger = new Mock<ILoggingService>();
        IServiceRule rule = new ServiceRule();
        IServiceScreen<FtpServerOptions> screen = new ServiceScreen<FtpServerOptions>(logger.Object);
        var vm = new FtpServerCreateViewModel(rule, screen, logger.Object)
        {
            ServiceName = "ftp",
            Port = 21,
            RootPath = "/tmp"
        };
        string? name = null;
        FtpServerOptions? options = null;
        vm.ServiceSaved += (n, o) => { name = n; options = o; };

        vm.SaveCommand.Execute(null);

        Assert.Equal("ftp", name);
        Assert.NotNull(options);
        Assert.Equal(21, options!.Port);
        Assert.Equal("/tmp", options.RootPath);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    public void CancelCommand_RaisesEditCancelled()
    {
        IServiceRule rule = new ServiceRule();
        IServiceScreen<FtpServerOptions> screen = new ServiceScreen<FtpServerOptions>();
        var vm = new FtpServerCreateViewModel(rule, screen);
        var cancelled = false;
        vm.EditCancelled += () => cancelled = true;

        vm.CancelCommand.Execute(null);

        Assert.True(cancelled);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    public void SettingInvalidPort_AddsError()
    {
        IServiceRule rule = new ServiceRule();
        IServiceScreen<FtpServerOptions> screen = new ServiceScreen<FtpServerOptions>();
        var vm = new FtpServerCreateViewModel(rule, screen);
        vm.Port = 0;
        Assert.True(vm.HasErrors);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    public void SettingEmptyServiceName_AddsError()
    {
        IServiceRule rule = new ServiceRule();
        IServiceScreen<FtpServerOptions> screen = new ServiceScreen<FtpServerOptions>();
        var vm = new FtpServerCreateViewModel(rule, screen);
        vm.ServiceName = string.Empty;
        Assert.True(vm.HasErrors);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    public void SaveCommand_DoesNotRaise_WhenInvalid()
    {
        IServiceRule rule = new ServiceRule();
        IServiceScreen<FtpServerOptions> screen = new ServiceScreen<FtpServerOptions>();
        var vm = new FtpServerCreateViewModel(rule, screen)
        {
            ServiceName = string.Empty,
            Port = 21,
            RootPath = "/tmp"
        };
        var raised = false;
        vm.ServiceSaved += (_, _) => raised = true;

        vm.SaveCommand.Execute(null);

        Assert.False(raised);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    public void AdvancedCommand_RaisesRequested()
    {
        IServiceRule rule = new ServiceRule();
        IServiceScreen<FtpServerOptions> screen = new ServiceScreen<FtpServerOptions>();
        var vm = new FtpServerCreateViewModel(rule, screen);
        var raised = false;
        vm.AdvancedConfigRequested += _ => raised = true;

        vm.AdvancedConfigCommand.Execute(null);

        Assert.True(raised);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    public void ServiceProvider_ResolvesFtpServerCreateViewModel()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IServiceRule, ServiceRule>();
        services.AddSingleton(typeof(IServiceScreen<>), typeof(ServiceScreen<>));
        services.AddSingleton<ILoggingService>(_ => new Mock<ILoggingService>().Object);
        services.AddTransient<FtpServerCreateViewModel>();

        using var provider = services.BuildServiceProvider();
        var vm = provider.GetService<FtpServerCreateViewModel>();

        Assert.NotNull(vm);
        ConsoleTestLogger.LogPass();
    }
}
