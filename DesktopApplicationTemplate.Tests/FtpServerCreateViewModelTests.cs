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
        var vm = new FtpServerCreateViewModel(new ServiceRule(), new ServiceScreen<FtpServerOptions>(logger.Object), logger.Object)
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
        var vm = new FtpServerCreateViewModel(new ServiceRule(), new ServiceScreen<FtpServerOptions>());
        var cancelled = false;
        vm.EditCancelled += () => cancelled = true;

        vm.CancelCommand.Execute(null);

        Assert.True(cancelled);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    public void SettingInvalidPort_AddsError()
    {
        var vm = new FtpServerCreateViewModel(new ServiceRule(), new ServiceScreen<FtpServerOptions>());
        vm.Port = 0;
        Assert.True(vm.HasErrors);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    public void SettingEmptyServiceName_AddsError()
    {
        var vm = new FtpServerCreateViewModel(new ServiceRule(), new ServiceScreen<FtpServerOptions>());
        vm.ServiceName = string.Empty;
        Assert.True(vm.HasErrors);
        ConsoleTestLogger.LogPass();
    }

    [Fact]
    public void SaveCommand_DoesNotRaise_WhenInvalid()
    {
        var vm = new FtpServerCreateViewModel(new ServiceRule(), new ServiceScreen<FtpServerOptions>())
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
        var vm = new FtpServerCreateViewModel(new ServiceRule(), new ServiceScreen<FtpServerOptions>());
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
        services.AddTransient<FtpServerCreateViewModel>();

        using var provider = services.BuildServiceProvider();
        var vm = provider.GetService<FtpServerCreateViewModel>();

        Assert.NotNull(vm);
        ConsoleTestLogger.LogPass();
    }
}
