using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.Service.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class DiContainerTests
{
    [Fact]
    public void ServiceProvider_BuildsSuccessfully()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IRichTextLogger, NullRichTextLogger>();
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<IMessageRoutingService, MessageRoutingService>();
        services.AddSingleton<SaveConfirmationHelper>();
        services.AddSingleton<IServiceRule, ServiceRule>();
        services.AddSingleton<MqttService>();
        services.AddSingleton<MqttTagSubscriptionsViewModel>();
        services.AddSingleton<ServiceMessageTableViewModel>();
        services.AddTransient<TcpCreateServiceViewModel>();
        services.AddTransient<TcpEditServiceViewModel>();
        services.AddTransient<TcpServiceMessagesViewModel>();
        services.Configure<MqttServiceOptions>(o =>
        {
            o.Host = "localhost";
            o.Port = 1883;
            o.ClientId = "client";
        });
        services.Configure<TcpServiceOptions>(o =>
        {
            o.Host = "localhost";
            o.Port = 5000;
            o.Mode = TcpServiceMode.Listening;
            o.UseUdp = false;
        });

        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetRequiredService<MqttTagSubscriptionsViewModel>());
        Assert.NotNull(provider.GetRequiredService<TcpCreateServiceViewModel>());
        Assert.NotNull(provider.GetRequiredService<TcpEditServiceViewModel>());
        Assert.NotNull(provider.GetRequiredService<TcpServiceMessagesViewModel>());
        Assert.NotNull(provider.GetRequiredService<ServiceMessageTableViewModel>());
    }
}
