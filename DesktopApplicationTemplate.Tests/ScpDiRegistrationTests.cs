using DesktopApplicationTemplate.Core.Services;
using DesktopApplicationTemplate.UI.Helpers;
using DesktopApplicationTemplate.UI.Services;
using DesktopApplicationTemplate.UI.ViewModels;
using DesktopApplicationTemplate.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DesktopApplicationTemplate.Tests;

public class ScpDiRegistrationTests
{
    [Fact]
    [TestCategory("CodexSafe")]
    public void ServiceProvider_Resolves_ScpTypes()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IRichTextLogger, NullRichTextLogger>();
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<SaveConfirmationHelper>();
        services.AddTransient<ScpCreateServiceViewModel>();
        services.AddTransient<ScpCreateServiceView>();
        services.AddTransient<ScpEditServiceViewModel>();
        services.AddTransient<ScpEditServiceView>();
        services.AddTransient<ScpAdvancedConfigViewModel>();
        services.AddTransient<ScpAdvancedConfigView>();
        services.AddOptions<ScpServiceOptions>();

        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetRequiredService<ScpCreateServiceViewModel>());
        Assert.NotNull(provider.GetRequiredService<ScpEditServiceViewModel>());
        Assert.NotNull(provider.GetRequiredService<ScpAdvancedConfigViewModel>());
    }
}
