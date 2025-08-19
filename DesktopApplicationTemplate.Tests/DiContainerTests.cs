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
        services.Configure<MqttServiceOptions>(o =>
        {
            o.Host = "localhost";
            o.Port = 1883;
            o.ClientId = "client";
        });

        using var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<MqttTagSubscriptionsViewModel>();
        Assert.NotNull(vm);
    }
}
