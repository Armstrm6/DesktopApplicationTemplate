using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class DiContainerTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    public void ServiceProvider_BuildsSuccessfully()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IRichTextLogger, NullRichTextLogger>();
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<IMessageRoutingService, MessageRoutingService>();
        services.AddSingleton<SaveConfirmationHelper>();
        services.AddSingleton<MqttService>();
        services.AddSingleton<MqttTagSubscriptionsViewModel>();
        services.AddTransient<TcpCreateServiceViewModel>();
        services.AddTransient<TcpEditServiceViewModel>();
        services.AddTransient<TcpAdvancedConfigViewModel>();
        services.AddTransient<TcpServiceMessagesViewModel>();
        services.AddSingleton<TcpServiceViewModel>();
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
        });

        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetRequiredService<MqttTagSubscriptionsViewModel>());
        Assert.NotNull(provider.GetRequiredService<TcpCreateServiceViewModel>());
        Assert.NotNull(provider.GetRequiredService<TcpEditServiceViewModel>());
        Assert.NotNull(provider.GetRequiredService<TcpAdvancedConfigViewModel>());
        Assert.NotNull(provider.GetRequiredService<TcpServiceMessagesViewModel>());
        Assert.NotNull(provider.GetRequiredService<TcpServiceViewModel>());
    }
}
